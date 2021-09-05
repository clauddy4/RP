using System;
using ClassLibrary;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Valuator.Pages
{
    public class SummaryModel : PageModel
    {
        private readonly ILogger<SummaryModel> _logger;
        private readonly IRedisRepository _redisRepository;

        public SummaryModel(ILogger<SummaryModel> logger, IRedisRepository redisRepository)
        {
            _logger = logger;
            _redisRepository = redisRepository;
        }

        public double Rank { get; set; }
        public double Similarity { get; set; }

        public void OnGet(string id)
        {
            _logger.LogDebug(id);

            Similarity = Convert.ToDouble(_redisRepository.GetDataFromDbByKey("SIMILARITY-" + id));

            string rankKey = "RANK-" + id;
            if (!_redisRepository.IsKeyExistInDb(rankKey))
            {
                _logger.LogWarning("Rank key {rankKey} doesn't exist", rankKey);
                return;
            }
            Rank = Convert.ToDouble(_redisRepository.GetDataFromDbByKey(rankKey));
        }
    }
}