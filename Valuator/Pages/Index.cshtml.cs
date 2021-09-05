using System;
using System.Linq;
using System.Text;
using ClassLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace Valuator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IRedisRepository _redisRepository;
        private const string TextPrefix = "TEXT-";

        public IndexModel(ILogger<IndexModel> logger, IRedisRepository redisRepository)
        {
            _logger = logger;
            _redisRepository = redisRepository;
        }

        public IActionResult OnPost(string text, string country)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();
            var segmentId = GetSegmentId(country);
            _redisRepository.SaveShardKey(id, segmentId);
            _logger.LogDebug("LOOKUP: {id}, {segmentId}", id, segmentId);

            string textKey = TextPrefix + id;
            _redisRepository.SaveDataToDb(textKey, text, id);

            string similarityKey = "SIMILARITY-" + id;
            var similarity = GetSimilarity(text, id, segmentId);
            _redisRepository.SaveDataToDb(similarityKey, similarity.ToString(), id);

            CreateEventForSimilarity(id, similarity);

            CreateRankCalculator(id);

            return Redirect($"summary?id={id}");
        }

        private int GetSimilarity(string text, string id, string segmentId)
        {
            var keys = _redisRepository.GetKeysFromDbByPrefix(TextPrefix, id, segmentId);
            return keys.Any(key => key != TextPrefix + id && _redisRepository.GetDataFromAllServers(key) == text) ? 1 : 0;
        }

        private void CreateRankCalculator(string id)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            using (var connection = connectionFactory.CreateConnection())
            {
                byte[] data = Encoding.UTF8.GetBytes(id);
                connection.Publish("valuator.processing.rank", data);

                connection.Drain();
                connection.Close();
            }
        }

        private void CreateEventForSimilarity(string id, int similarity)
        {
            string message = $"Event: SimilarityCalculated, context id: {id}, similarity: {similarity}";
            ConnectionFactory connectionFactory = new ConnectionFactory();
            using (var connection = connectionFactory.CreateConnection())
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                connection.Publish("valuator.processing.similarityCalculated", data);

                connection.Drain();
                connection.Close();
            }
        }

        private string GetSegmentId(string country)
        {
            return country switch
            {
                "Russia" => Constants.SegmentRus,
                "France" or "Germany" => Constants.SegmentEu,
                "USA" or "India" => Constants.SegmentOther,
                _ => ""
            };
        }
    }
}