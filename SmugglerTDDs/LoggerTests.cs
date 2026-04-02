using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Smuggler.Common;

namespace SmugglerTDDs;

/// <summary>
/// C0105: SmugLogger 설정 및 필터 동작 검증 테스트
/// </summary>
public class LoggerTests
{
    // -------------------------------------------------------------------------
    // C0103: SmugLoggerConfiguration 기본값 검증
    // -------------------------------------------------------------------------

    [Fact]
    public void Configuration_DefaultMinLevel_IsInformation()
    {
        var config = new SmugLoggerConfiguration();

        Assert.Equal(LogLevel.Information, config.MinLevel);
    }

    [Fact]
    public void Configuration_DefaultEventId_IsZero()
    {
        var config = new SmugLoggerConfiguration();

        Assert.Equal(0, config.EventId);
    }

    [Fact]
    public void Configuration_DefaultColorMap_ContainsAllLevels()
    {
        var config = new SmugLoggerConfiguration();
        var expectedLevels = new[]
        {
            LogLevel.Trace,
            LogLevel.Debug,
            LogLevel.Information,
            LogLevel.Warning,
            LogLevel.Error,
            LogLevel.Critical,
        };

        foreach (LogLevel level in expectedLevels)
            Assert.True(config.LogLevelToColorMap.ContainsKey(level), $"{level}이 색상 맵에 없음");
    }

    // -------------------------------------------------------------------------
    // C0102: IsEnabled - MinLevel 필터 검증
    // -------------------------------------------------------------------------

    [Fact]
    public void IsEnabled_BelowMinLevel_ReturnsFalse()
    {
        var config = new SmugLoggerConfiguration { MinLevel = LogLevel.Warning };
        var logger = new SmugLogger("Test", () => config);

        Assert.False(logger.IsEnabled(LogLevel.Trace));
        Assert.False(logger.IsEnabled(LogLevel.Debug));
        Assert.False(logger.IsEnabled(LogLevel.Information));
    }

    [Fact]
    public void IsEnabled_AtOrAboveMinLevel_ReturnsTrue()
    {
        var config = new SmugLoggerConfiguration { MinLevel = LogLevel.Warning };
        var logger = new SmugLogger("Test", () => config);

        Assert.True(logger.IsEnabled(LogLevel.Warning));
        Assert.True(logger.IsEnabled(LogLevel.Error));
        Assert.True(logger.IsEnabled(LogLevel.Critical));
    }

    [Fact]
    public void IsEnabled_LevelNotInColorMap_ReturnsFalse()
    {
        var config = new SmugLoggerConfiguration
        {
            MinLevel = LogLevel.Trace,
            LogLevelToColorMap = new Dictionary<LogLevel, ConsoleColor>
            {
                [LogLevel.Information] = ConsoleColor.Green,
            },
        };
        var logger = new SmugLogger("Test", () => config);

        Assert.False(logger.IsEnabled(LogLevel.Debug));
        Assert.True(logger.IsEnabled(LogLevel.Information));
    }

    // -------------------------------------------------------------------------
    // C0102: Log - EventId 필터 검증
    // -------------------------------------------------------------------------

    [Fact]
    public void Log_EventIdZero_OutputsAllEventIds()
    {
        var config = new SmugLoggerConfiguration { EventId = 0 };
        var logger = new SmugLogger("Test", () => config);

        string output = CaptureConsole(() =>
        {
            logger.Log(LogLevel.Information, new EventId(1), "msg1", null, (s, _) => s);
            logger.Log(LogLevel.Information, new EventId(99), "msg99", null, (s, _) => s);
        });

        Assert.Contains("msg1", output);
        Assert.Contains("msg99", output);
    }

    [Fact]
    public void Log_SpecificEventId_OnlyOutputsMatchingEventId()
    {
        var config = new SmugLoggerConfiguration { EventId = 5 };
        var logger = new SmugLogger("Test", () => config);

        string output = CaptureConsole(() =>
        {
            logger.Log(LogLevel.Information, new EventId(5), "matched", null, (s, _) => s);
            logger.Log(LogLevel.Information, new EventId(9), "notmatched", null, (s, _) => s);
        });

        Assert.Contains("matched", output);
        Assert.DoesNotContain("notmatched", output);
    }

    // -------------------------------------------------------------------------
    // C0102: Log - 출력 포맷 검증
    // -------------------------------------------------------------------------

    [Fact]
    public void Log_OutputFormat_ContainsTimestampLevelCategory()
    {
        var config = new SmugLoggerConfiguration();
        var logger = new SmugLogger("MyCategory", () => config);

        string output = CaptureConsole(() =>
        {
            logger.Log(LogLevel.Information, new EventId(0), "hello", null, (s, _) => s);
        });

        // 타임스탬프 형식 확인 (yyyy-MM-dd 패턴)
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}", output);
        // 레벨 태그 확인
        Assert.Contains("[INF]", output);
        // 카테고리 확인
        Assert.Contains("MyCategory", output);
        // 메시지 확인
        Assert.Contains("hello", output);
    }

    [Fact]
    public void Log_WithException_OutputsExceptionInfo()
    {
        var config = new SmugLoggerConfiguration();
        var logger = new SmugLogger("Test", () => config);
        var ex = new InvalidOperationException("test exception");

        string output = CaptureConsole(() =>
        {
            logger.Log(LogLevel.Error, new EventId(0), "error msg", ex, (s, e) => s);
        });

        Assert.Contains("Exception", output);
        Assert.Contains("test exception", output);
    }

    [Fact]
    public void Log_WithNonZeroEventId_OutputsEventIdInFormat()
    {
        var config = new SmugLoggerConfiguration();
        var logger = new SmugLogger("Test", () => config);

        string output = CaptureConsole(() =>
        {
            logger.Log(LogLevel.Information, new EventId(42), "msg", null, (s, _) => s);
        });

        Assert.Contains("EventId:42", output);
    }

    // -------------------------------------------------------------------------
    // 헬퍼
    // -------------------------------------------------------------------------

    private static string CaptureConsole(Action action)
    {
        TextWriter original = Console.Out;
        using var sw = new StringWriter();
        Console.SetOut(sw);
        try
        {
            action();
        }
        finally
        {
            Console.SetOut(original);
        }
        return sw.ToString();
    }
}
