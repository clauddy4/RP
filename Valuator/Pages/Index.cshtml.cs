using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
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

        public IActionResult OnPost(string text)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string textKey = TextPrefix + id;
            _redisRepository.SaveDataToDb(textKey, text);

            string rankKey = "RANK-" + id;
            var rank = GetRank(text);
            _redisRepository.SaveDataToDb(rankKey, rank.ToString());

            string similarityKey = "SIMILARITY-" + id;
            var similarity = GetSimilarity(text, id);
            _redisRepository.SaveDataToDb(similarityKey, similarity.ToString());

            return Redirect($"summary?id={id}");
        }

        private static double GetRank(string text)
        {
            var notLetterCount = text.Count(ch => !char.IsLetter(ch));
            return notLetterCount / (double)text.Length;
        }

        private int GetSimilarity(string text, string id)
        {
            var keys = _redisRepository.GetKeysFromDbByPrefix(TextPrefix);
            return keys.Any(key => key != TextPrefix + id && _redisRepository.GetDataFromDbByKey(key) == text) ? 1 : 0;
        }
    }
}