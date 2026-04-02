using Microsoft.Extensions.Logging;
using Smuggler.Common;

namespace TestConsole;

/// <summary>
/// C0106: SmugLogger 콘솔 출력 및 log4net 파일 롤링 예제
/// </summary>
internal static class Logger
{
    internal static void Run()
    {
        Console.WriteLine("=== Logger 예제 시작 ===");
        Console.WriteLine();

        // 1. 기본 설정으로 로거 팩토리 생성 (콘솔: INF 이상 / 파일: log4net.config 참조)
        using ILoggerFactory factory = SmugLoggerInitializer.Create();
        ILogger logger = factory.CreateLogger("TestConsole.Logger");

        Console.WriteLine("--- 기본 설정 (MinLevel=Information) ---");
        logger.LogTrace("Trace 메시지 - 출력되지 않음");
        logger.LogDebug("Debug 메시지 - 출력되지 않음");
        logger.LogInformation("Information 메시지");
        logger.LogWarning("Warning 메시지");
        logger.LogError("Error 메시지");
        logger.LogCritical("Critical 메시지");

        Console.WriteLine();

        // 2. MinLevel을 Debug로 낮춘 설정
        var debugConfig = new SmugLoggerConfiguration { MinLevel = LogLevel.Debug };
        using ILoggerFactory debugFactory = SmugLoggerInitializer.Create(debugConfig);
        ILogger debugLogger = debugFactory.CreateLogger("TestConsole.Logger");

        Console.WriteLine("--- MinLevel=Debug 설정 ---");
        debugLogger.LogDebug("Debug 메시지 - 이제 출력됨");
        debugLogger.LogInformation("Information 메시지");

        Console.WriteLine();

        // 3. EventId 필터 예제
        var eventConfig = new SmugLoggerConfiguration { EventId = 100 };
        using ILoggerFactory eventFactory = SmugLoggerInitializer.Create(eventConfig);
        ILogger eventLogger = eventFactory.CreateLogger("TestConsole.Logger");

        Console.WriteLine("--- EventId=100 필터 설정 ---");
        eventLogger.Log(LogLevel.Information, new EventId(100), "EventId 100 - 출력됨");
        eventLogger.Log(LogLevel.Information, new EventId(200), "EventId 200 - 출력되지 않음");

        Console.WriteLine();

        // 4. 예외 포함 로그
        Console.WriteLine("--- 예외 포함 로그 ---");
        try
        {
            throw new InvalidOperationException("예제 예외 발생");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "예외가 발생했습니다.");
        }

        Console.WriteLine();
        Console.WriteLine("=== Logger 예제 종료 ===");
    }
}
