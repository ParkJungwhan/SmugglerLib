using System;
using System.Collections.Generic;
using System.Globalization;

namespace Smuggler.Common.ExcelLib;

/// <summary>
/// C0503: 엑셀 셀 값의 타입 변환 정책을 정의하는 유틸리티 클래스.
/// </summary>
/// <remarks>
/// <b>null 정책:</b> null, 빈 문자열, "-", "null"(대소문자 무관)은 null로 처리한다.<br/>
/// <b>타입 힌트 정책 (README 칼럼타입 기준):</b><br/>
/// - bigint → <see cref="long"/><br/>
/// - int    → <see cref="int"/><br/>
/// - float  → <see cref="double"/> (InvariantCulture)<br/>
/// - nvarchar / varchar / ntext → <see cref="string"/><br/>
/// - date     → <see cref="DateTime"/>? (yyyy-MM-dd)<br/>
/// - datetime → <see cref="DateTime"/>? (yyyy-MM-dd HH:mm:ss)<br/>
/// - time     → <see cref="TimeSpan"/>? (HH:mm:ss.fff)<br/>
/// - 그 외    → <see cref="string"/> 그대로 반환
/// </remarks>
public static class ExcelCellConverter
{
    private static readonly HashSet<string> NullSentinels =
        new(StringComparer.OrdinalIgnoreCase) { "", "-", "null" };

    /// <summary>
    /// typeHint와 rawValue를 받아 적절한 .NET 객체로 변환한다.
    /// </summary>
    public static object? Convert(string? typeHint, string? rawValue)
    {
        if (IsNullValue(rawValue))
            return null;

        string v = rawValue!.Trim();

        return typeHint?.Trim().ToLowerInvariant() switch
        {
            "bigint"                           => long.TryParse(v, out long l) ? l : null,
            "int"                              => int.TryParse(v, out int i) ? i : null,
            "float"                            => double.TryParse(v, NumberStyles.Float,
                                                     CultureInfo.InvariantCulture, out double d) ? d : null,
            "nvarchar" or "varchar" or "ntext" => v,
            "date"                             => DateTime.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None, out DateTime dt) ? dt : null,
            "datetime"                         => DateTime.TryParse(v, CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None, out DateTime dt2) ? dt2 : null,
            "time"                             => TimeSpan.TryParse(v, out TimeSpan ts) ? ts : null,
            _                                  => v,
        };
    }

    /// <summary>
    /// rawValue를 targetType에 맞게 변환한다. MapTo&lt;T&gt; 내부에서 사용한다.
    /// </summary>
    public static object? ConvertToType(string? rawValue, Type targetType)
    {
        Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (IsNullValue(rawValue))
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        string v = rawValue!.Trim();

        try
        {
            if (underlying == typeof(string))   return v;
            if (underlying == typeof(int))      return int.Parse(v, CultureInfo.InvariantCulture);
            if (underlying == typeof(long))     return long.Parse(v, CultureInfo.InvariantCulture);
            if (underlying == typeof(double))   return double.Parse(v, CultureInfo.InvariantCulture);
            if (underlying == typeof(float))    return float.Parse(v, CultureInfo.InvariantCulture);
            if (underlying == typeof(bool))     return v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (underlying == typeof(DateTime)) return DateTime.Parse(v, CultureInfo.InvariantCulture);
            if (underlying == typeof(TimeSpan)) return TimeSpan.Parse(v, CultureInfo.InvariantCulture);
            return System.Convert.ChangeType(v, underlying, CultureInfo.InvariantCulture);
        }
        catch
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }

    private static bool IsNullValue(string? value) =>
        value is null || NullSentinels.Contains(value.Trim());
}
