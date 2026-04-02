using Microsoft.Extensions.Logging;
using Smuggler.Common;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace SmugglerTDDs;

public class SmugLoggerTests
{
    [Fact]
    public void DefaultConfiguration_UsesExpectedPolicies()
    {
        SmugLoggerConfiguration configuration = new();

        Assert.Equal(LogLevel.Information, configuration.MinimumLogLevel);
        Assert.Empty(configuration.AllowedEventIds);
        Assert.Equal(ConsoleColor.Green, configuration.LogLevelToColorMap[LogLevel.Information]);
        Assert.False(configuration.ShouldLog(LogLevel.Debug, new EventId(10)));
        Assert.True(configuration.ShouldLog(LogLevel.Warning, new EventId(10)));
    }

    [Fact]
    public void Logger_DoesNotWrite_WhenBelowMinimumLogLevel()
    {
        string output = CaptureConsole(() =>
        {
            SmugLogger logger = new("Tests.Minimum", () => new SmugLoggerConfiguration
            {
                MinimumLogLevel = LogLevel.Warning,
            });

            logger.Log(
                LogLevel.Information,
                new EventId(7),
                "ignored message",
                null,
                static (state, _) => state);
        });

        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void Logger_DoesNotWrite_WhenEventIdIsFilteredOut()
    {
        string output = CaptureConsole(() =>
        {
            SmugLogger logger = new("Tests.Filter", () => new SmugLoggerConfiguration
            {
                MinimumLogLevel = LogLevel.Trace,
                AllowedEventIds = [42],
            });

            logger.Log(
                LogLevel.Warning,
                new EventId(99),
                "filtered message",
                null,
                static (state, _) => state);
        });

        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void Logger_WritesCategoryEventIdAndException()
    {
        string output = CaptureConsole(() =>
        {
            SmugLogger logger = new("Tests.Output", () => new SmugLoggerConfiguration
            {
                MinimumLogLevel = LogLevel.Trace,
            });

            InvalidOperationException exception = new("boom");

            logger.Log(
                LogLevel.Error,
                new EventId(42),
                "visible message",
                exception,
                static (state, ex) => $"{state} :: {ex?.Message}");
        });

        Assert.Contains("Tests.Output", output);
        Assert.Contains("EventId:42", output);
        Assert.Contains("visible message :: boom", output);
        Assert.Contains("System.InvalidOperationException", output);
    }

    [Fact]
    public void LoggingBuilder_AddSmugLogger_CreatesWorkingLogger()
    {
        string output = CaptureConsole(() =>
        {
            using ILoggerFactory factory = LoggerFactory.Create(builder =>
                builder.AddSmugLogger(config =>
                {
                    config.MinimumLogLevel = LogLevel.Trace;
                    config.AllowedEventIds = [7];
                }));

            ILogger logger = factory.CreateLogger("Tests.Factory");
            logger.LogInformation(new EventId(7), "factory message");
        });

        Assert.Contains("Tests.Factory", output);
        Assert.Contains("factory message", output);
        Assert.Contains("EventId:7", output);
    }

    private static string CaptureConsole(Action action)
    {
        TextWriter originalWriter = Console.Out;
        StringWriter writer = new();

        try
        {
            Console.SetOut(writer);
            action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(originalWriter);
            writer.Dispose();
        }
    }
}
