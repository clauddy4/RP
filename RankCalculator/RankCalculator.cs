using ClassLibrary;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            var subscription = _connection.SubscribeAsync("valuator.processing.rank", "rank_calculator", async (sender, args) =>
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

                await CreateEventForRank(id, rank);
            });

            subscription.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            subscription.Unsubscribe();

            _connection.Drain();
            _connection.Close();        
        }

        private async Task CreateEventForRank(string id, string rank)
        {
            string message = $"Event: RankCalculated, context id: {id}, rank: {rank}";
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            ConnectionFactory connectionFactory = new ConnectionFactory();
            using (var connection = connectionFactory.CreateConnection())
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    connection.Publish("rankCalculator.processing.rankCalculated", data);
                    await Task.Delay(1000);
                }
                connection.Drain();
                connection.Close();
            }
        }

        private static double GetRank(string text)
        {
            var notLetterCount = text.Count(ch => !char.IsLetter(ch));
            return notLetterCount / (double)text.Length;
        }
    }
}
