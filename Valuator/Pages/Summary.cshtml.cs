using System;
using System.Threading;
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

            Similarity = Convert.ToDouble(_redisRepository.GetDataFromDbByKey("SIMILARITY-" + id, id));

            var segmentId = _redisRepository.GetSegmentIdFromDb(id);
            _logger.LogDebug("LOOKUP: {textId}, {segmentId}", id, segmentId);

            string rankKey = "RANK-" + id;
            for (int retryCount = 0; retryCount < 10; retryCount++)
            {
                Thread.Sleep(100);
                if (_redisRepository.IsKeyExistInDb(rankKey, id))
                {
                    Rank = Convert.ToDouble(_redisRepository.GetDataFromDbByKey(rankKey, id));
                    return;
                }
            }
            Rank = Convert.ToDouble(_redisRepository.GetDataFromDbByKey(rankKey, id));
        }
    }
}