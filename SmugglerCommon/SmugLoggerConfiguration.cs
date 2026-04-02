using Microsoft.Extensions.Logging;

namespace Smuggler.Common;

/// <summary>
/// SmugLogger 콘솔 출력에 대한 설정 클래스.<br/>
/// 최소 로그 레벨, 색상 맵, EventId 필터를 제공한다.
/// </summary>
public class SmugLoggerConfiguration
{
    /// <summary>출력할 최소 로그 레벨. 이 레벨 미만의 로그는 출력하지 않는다.</summary>
    public LogLevel MinLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// EventId 필터. 0이면 모든 EventId의 로그를 출력하고,
    /// 0이 아닌 값이면 해당 EventId의 로그만 출력한다.
    /// </summary>
    public int EventId { get; set; } = 0;

    /// <summary>
    /// 로그 레벨별 콘솔 출력 색상 맵.<br/>
    /// 맵에 없는 로그 레벨은 IsEnabled에서 false를 반환하여 출력하지 않는다.
    /// </summary>
    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
    {
        [LogLevel.Trace]       = ConsoleColor.Gray,
        [LogLevel.Debug]       = ConsoleColor.Cyan,
        [LogLevel.Information] = ConsoleColor.Green,
        [LogLevel.Warning]     = ConsoleColor.Yellow,
        [LogLevel.Error]       = ConsoleColor.Red,
        [LogLevel.Critical]    = ConsoleColor.DarkRed,
    };
}
