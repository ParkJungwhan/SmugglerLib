using Microsoft.Extensions.Logging;
using Smuggler.Common;

namespace TestConsole;

public static class LoggerExample
{
    public static void Run()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddSmugLoggerWithLog4Net(configure: config =>
            {
                config.MinimumLogLevel = LogLevel.Trace;
                config.AllowedEventIds = [100, 200];
            });
        });

        ILogger logger = factory.CreateLogger("TestConsole.LoggerExample");

        logger.LogInformation(new EventId(100), "C0100 info message");
        logger.LogWarning(new EventId(200), "C0100 warning message");
        logger.LogError(new EventId(999), new InvalidOperationException("filtered out"), "This message should not be printed because EventId 999 is filtered.");
    }
}
