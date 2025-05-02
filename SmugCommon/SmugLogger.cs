using Microsoft.Extensions.Logging;

namespace SmugCommon
{
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

        public bool IsEnabled(LogLevel logLevel) => _getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

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
            if (config.EventId == 0 || config.EventId == eventId.Id)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
                Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

                Console.ForegroundColor = originalColor;
                Console.Write($"     {_name} - ");

                Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
                Console.Write($"{formatter(state, exception)}");

                Console.ForegroundColor = originalColor;
                Console.WriteLine();
            }
        }
    }
}