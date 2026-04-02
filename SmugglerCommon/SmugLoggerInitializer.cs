using Microsoft.Extensions.Logging;

namespace Smuggler.Common;

/// <summary>
/// Microsoft.Extensions.Logging과 log4net을 함께 초기화하는 헬퍼 클래스.<br/>
/// SmugLogger(콘솔)와 log4net(파일)을 하나의 ILoggerFactory로 구성한다.
/// </summary>
public static class SmugLoggerInitializer
{
    /// <summary>
    /// SmugLogger(콘솔)와 log4net(파일) Provider를 포함한 ILoggerFactory를 생성한다.
    /// </summary>
    /// <param name="consoleConfig">콘솔 출력 설정. null이면 기본값을 사용한다.</param>
    /// <param name="log4netConfigFile">log4net 설정 파일 경로. 기본값은 "log4net.config"이다.</param>
    public static ILoggerFactory Create(
        SmugLoggerConfiguration? consoleConfig = null,
        string log4netConfigFile = "log4net.config")
    {
        SmugLoggerConfiguration config = consoleConfig ?? new SmugLoggerConfiguration();

        return LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(config.MinLevel)
                .AddSmugLogger(() => config)
                .AddLog4Net(log4netConfigFile);
        });
    }
}
