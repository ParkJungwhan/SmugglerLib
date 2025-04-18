using Microsoft.Extensions.Logging;

namespace SmugCommon
{
    public class SmugLoggerConfiguration
    {
        public int EventId { get; set; }
        public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new();
    }
}