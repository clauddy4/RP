using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Valuator.Data.Repositories;

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
            Rank = Convert.ToDouble(_redisRepository.GetDataFromDbByKey("RANK-" + id));
            Similarity = Convert.ToDouble(_redisRepository.GetDataFromDbByKey("SIMILARITY-" + id));
        }
    }
}