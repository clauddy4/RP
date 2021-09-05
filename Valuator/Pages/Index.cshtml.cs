using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NATS.Client;
using Valuator.Data.Repositories;

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

        public async Task<IActionResult> OnPost(string text)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string textKey = TextPrefix + id;
            _redisRepository.SaveDataToDb(textKey, text);

            string similarityKey = "SIMILARITY-" + id;
            var similarity = GetSimilarity(text, id);
            _redisRepository.SaveDataToDb(similarityKey, similarity.ToString());

            await CreateRankCalculator(id);

            return Redirect($"summary?id={id}");
        }

        private int GetSimilarity(string text, string id)
        {
            var keys = _redisRepository.GetKeysFromDbByPrefix(TextPrefix);
            return keys.Any(key => key != TextPrefix + id && _redisRepository.GetDataFromDbByKey(key) == text) ? 1 : 0;
        }

        private async Task CreateRankCalculator(string id)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            ConnectionFactory connectionFactory = new ConnectionFactory();
            using (var connection = connectionFactory.CreateConnection())
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    byte[] data = Encoding.UTF8.GetBytes(id);
                    connection.Publish("valuator.processing.rank", data);
                    await Task.Delay(1000);
                }
                connection.Drain();
                connection.Close();
            }
        }
    }
}