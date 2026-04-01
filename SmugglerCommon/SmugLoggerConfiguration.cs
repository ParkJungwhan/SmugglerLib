using Microsoft.Extensions.Logging;

namespace Smuggler.Common;

public class SmugLoggerConfiguration
{
    public int EventId { get; set; }
    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new();
}