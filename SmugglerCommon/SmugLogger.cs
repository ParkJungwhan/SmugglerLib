using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore;

namespace Smuggler.Common;

public static class SmugLoggerExtensions
{
    public static ILoggingBuilder AddSmugLogger(
            this ILoggingBuilder builder,
            Func<SmugLoggerConfiguration> getCurrentConfig) =>
        builder.AddProvider(new SmugLoggerProvider(getCurrentConfig));

    public static ILoggingBuilder AddSmugLogger(
            this ILoggingBuilder builder,
            Action<SmugLoggerConfiguration>? configure = null)
    {
        SmugLoggerConfiguration configuration = new();
        configure?.Invoke(configuration);
        return builder.AddSmugLogger(() => configuration);
    }

    public static ILoggingBuilder AddSmugLoggerWithLog4Net(
            this ILoggingBuilder builder,
            string configFilePath = "log4net.config",
            Action<SmugLoggerConfiguration>? configure = null)
    {
        builder.AddLog4Net(configFilePath);
        return builder.AddSmugLogger(configure);
    }
}

public class SmugLoggerProvider : ILoggerProvider
{
    private readonly Func<SmugLoggerConfiguration> _getCurrentConfig;

    public SmugLoggerProvider(Func<SmugLoggerConfiguration> getCurrentConfig)
    {
        _getCurrentConfig = getCurrentConfig;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SmugLogger(categoryName, _getCurrentConfig);
    }

    public void Dispose()
    {
    }
}

public class SmugLogger : ILogger
{
    private readonly string _name;
    private readonly Func<SmugLoggerConfiguration> _getCurrentConfig;

    public SmugLogger(
            string name,
            Func<SmugLoggerConfiguration> getCurrentConfig) =>
        (_name, _getCurrentConfig) = (name, getCurrentConfig);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel)
    {
        SmugLoggerConfiguration config = _getCurrentConfig();
        return logLevel != LogLevel.None && logLevel >= config.MinimumLogLevel && config.LogLevelToColorMap.ContainsKey(logLevel);
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        SmugLoggerConfiguration config = _getCurrentConfig();
        if (!config.ShouldLog(logLevel, eventId))
        {
            return;
        }

        ConsoleColor originalColor = Console.ForegroundColor;
        ConsoleColor color = config.LogLevelToColorMap[logLevel];
        string timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string message = formatter(state, exception);
        string eventIdSegment = eventId.Id == 0 ? "EventId:none" : $"EventId:{eventId.Id}";

        Console.ForegroundColor = color;
        Console.Write($"[{timestamp}] [{logLevel,-11}] ");

        Console.ForegroundColor = originalColor;
        Console.Write($"{_name} {eventIdSegment} - ");

        Console.ForegroundColor = color;
        Console.Write(message);

        Console.ForegroundColor = originalColor;

        if (exception is not null)
        {
            Console.WriteLine();
            Console.WriteLine(exception);
        }
        else
        {
            Console.WriteLine();
        }
    }
}
