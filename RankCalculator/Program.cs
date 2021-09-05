using ClassLibrary;
using Microsoft.Extensions.Logging;

namespace RankCalculator
{
    class Program
    {
        static void Main()
        {
            using (var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug)))
            {
                var redisRepository = new RedisRepository();
                var rankCalculator = new RankCalculator(loggerFactory.CreateLogger<RankCalculator>(), redisRepository);
                rankCalculator.Run();
            }
        }
    }
}
