using Microsoft.Extensions.Logging;
using NATS.Client;
using System;
using System.Linq;
using System.Text;
using Valuator.Data.Repositories;

namespace RankCalculator
{
    public class RankCalculator
    {
        private readonly IRedisRepository _redisRepository;
        private readonly ILogger _logger;
        private readonly IConnection _connection;

        public RankCalculator(ILogger<RankCalculator> logger, IRedisRepository redisRepository)
        {
            _logger = logger;
            _redisRepository = redisRepository;
            _connection = new ConnectionFactory().CreateConnection();
        }

        public void Run()
        {
            var subscription = _connection.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args) =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);
                string textKey = "TEXT-" + id;

                if (!_redisRepository.IsKeyExistInDb(textKey))
                {
                    _logger.LogWarning("Text key {textKey} doesn't exist", textKey);
                    return;
                }

                string text = _redisRepository.GetDataFromDbByKey(textKey);
                string rankKey = "RANK-" + id;
                string rank = GetRank(text).ToString();

                _logger.LogDebug("Rank {rank} with key {rankKey} by text id {id}", rank, rankKey, id);

                _redisRepository.SaveDataToDb(rankKey, rank);
            });

            subscription.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            subscription.Unsubscribe();

            _connection.Drain();
            _connection.Close();
        }

        private static double GetRank(string text)
        {
            var notLetterCount = text.Count(ch => !char.IsLetter(ch));
            return notLetterCount / (double)text.Length;
        }
    }
}
