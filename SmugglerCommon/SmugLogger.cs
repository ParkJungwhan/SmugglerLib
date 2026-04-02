using Microsoft.Extensions.Logging;

namespace Smuggler.Common;

public static class SmugLoggerExtensions
{
    public static ILoggingBuilder AddSmugLogger(
            this ILoggingBuilder builder,
            Func<SmugLoggerConfiguration> getCurrentConfig) =>
        builder.AddProvider(new SmugLoggerProvider(getCurrentConfig));
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
        // No resources to dispose
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

    /// <summary>
    /// logLevel이 MinLevel 이상이고 LogLevelToColorMap에 등록된 경우에만 true를 반환한다.
    /// </summary>
    public bool IsEnabled(LogLevel logLevel)
    {
        SmugLoggerConfiguration config = _getCurrentConfig();
        if (logLevel < config.MinLevel)
            return false;
        return config.LogLevelToColorMap.ContainsKey(logLevel);
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        SmugLoggerConfiguration config = _getCurrentConfig();

        // EventId 필터: config.EventId가 0이면 모두 출력, 아니면 일치하는 경우만 출력
        if (config.EventId != 0 && config.EventId != eventId.Id)
            return;

        ConsoleColor originalColor = Console.ForegroundColor;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string levelTag = GetLevelTag(logLevel);
        ConsoleColor levelColor = config.LogLevelToColorMap[logLevel];

        // 형식: [yyyy-MM-dd HH:mm:ss.fff] [LVL] [EventId:N] CategoryName: message
        Console.ForegroundColor = levelColor;
        Console.Write($"[{timestamp}] [{levelTag}]");

        if (eventId.Id != 0)
            Console.Write($" [EventId:{eventId.Id}]");

        Console.ForegroundColor = originalColor;
        Console.Write($" {_name}: ");

        Console.ForegroundColor = levelColor;
        Console.Write(formatter(state, exception));

        if (exception != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write($"\n  Exception: {exception}");
        }

        Console.ForegroundColor = originalColor;
        Console.WriteLine();
    }

    private static string GetLevelTag(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace       => "TRC",
        LogLevel.Debug       => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning     => "WRN",
        LogLevel.Error       => "ERR",
        LogLevel.Critical    => "CRT",
        _                    => "???",
    };
}
