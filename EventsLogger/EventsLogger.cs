using Microsoft.Extensions.Logging;
using NATS.Client;
using System;
using System.Text;

namespace EventsLogger
{
    public class EventsLogger
    {
        private ILogger _logger;
        private readonly IConnection _connection;

        public EventsLogger(ILogger<EventsLogger> logger)
        {
            _logger = logger;
            _connection = new ConnectionFactory().CreateConnection();
        }

        public void Run()
        {
            var subscription = _connection.SubscribeAsync("rankCalculator.processing.rankCalculated", (sender, args) =>
            {
                string message = Encoding.UTF8.GetString(args.Message.Data);
                _logger.LogDebug(message);
            });

            subscription = _connection.SubscribeAsync("valuator.processing.similarityCalculated", (sender, args) =>
            {
                string message = Encoding.UTF8.GetString(args.Message.Data);
                _logger.LogDebug(message);
            });

            subscription.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            subscription.Unsubscribe();

            _connection.Drain();
            _connection.Close();
        }
    }
}
