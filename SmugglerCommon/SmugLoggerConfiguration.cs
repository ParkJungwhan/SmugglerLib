using Microsoft.Extensions.Logging;

namespace Smuggler.Common;

public class SmugLoggerConfiguration
{
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    public HashSet<int> AllowedEventIds { get; set; } = [];

    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = CreateDefaultColorMap();

    public bool ShouldLog(LogLevel logLevel, EventId eventId)
    {
        if (logLevel == LogLevel.None || logLevel < MinimumLogLevel)
        {
            return false;
        }

        return AllowedEventIds.Count == 0 || AllowedEventIds.Contains(eventId.Id);
    }

    public static Dictionary<LogLevel, ConsoleColor> CreateDefaultColorMap() =>
        new()
        {
            [LogLevel.Trace] = ConsoleColor.DarkGray,
            [LogLevel.Debug] = ConsoleColor.Gray,
            [LogLevel.Information] = ConsoleColor.Green,
            [LogLevel.Warning] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Red,
            [LogLevel.Critical] = ConsoleColor.Magenta,
        };
}
