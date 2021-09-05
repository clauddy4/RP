using Microsoft.Extensions.Logging;

namespace EventsLogger
{
    class Program
    {
        static void Main()
        {
            using (var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug)))
            {
                var eventsLogger = new EventsLogger(loggerFactory.CreateLogger<EventsLogger>());
                eventsLogger.Run();
            }
        }
    }
}
