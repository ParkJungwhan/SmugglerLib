using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Smuggler.Common.ExcelLib;

/// <summary>
/// 시트 이름, 헤더 목록, 행 데이터를 보유하는 엑셀 시트 데이터 컨테이너.
/// </summary>
public class ExcelSheet
{
    private readonly List<IReadOnlyDictionary<string, string?>> _rows;

    /// <summary>시트 이름.</summary>
    public string Name { get; }

    /// <summary>헤더(컬럼명) 목록. '#' 접두사 컬럼은 포함되지 않는다.</summary>
    public IReadOnlyList<string> Headers { get; }

    /// <summary>
    /// 행 데이터 목록. 각 행은 헤더명 → 셀 문자열 값의 딕셔너리이다.<br/>
    /// 빈 셀이나 null 센티널("-", "null")은 null로 저장된다.
    /// </summary>
    public IReadOnlyList<IReadOnlyDictionary<string, string?>> Rows => _rows.AsReadOnly();

    internal ExcelSheet(string name, List<string> headers, List<IReadOnlyDictionary<string, string?>> rows)
    {
        Name    = name;
        Headers = headers.AsReadOnly();
        _rows   = rows;
    }

    /// <summary>
    /// 헤더 이름과 T의 프로퍼티 이름을 매핑(대소문자 무관)하여 행 데이터를 T 인스턴스로 변환한다.<br/>
    /// 셀 값 변환은 <see cref="ExcelCellConverter.ConvertToType"/>을 사용한다.
    /// </summary>
    public IEnumerable<T> MapTo<T>() where T : new()
    {
        Dictionary<string, PropertyInfo> props = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        foreach (IReadOnlyDictionary<string, string?> row in _rows)
        {
            T obj = new T();
            foreach (KeyValuePair<string, string?> cell in row)
            {
                if (!props.TryGetValue(cell.Key, out PropertyInfo? prop))
                    continue;

                object? converted = ExcelCellConverter.ConvertToType(cell.Value, prop.PropertyType);

                if (converted is not null || !prop.PropertyType.IsValueType)
                    prop.SetValue(obj, converted);
            }
            yield return obj;
        }
    }
}
