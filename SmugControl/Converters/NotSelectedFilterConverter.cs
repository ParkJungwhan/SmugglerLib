using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace SmugControl.Converters;

public sealed class NotSelectedFilterConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return Enumerable.Empty<object>();
        if (values[0] is not IEnumerable items) return Enumerable.Empty<object>();

        var selected = values[1];

        var list = new List<object>();
        foreach (var item in items)
        {
            if (selected != null && (ReferenceEquals(item, selected) || (item?.Equals(selected) ?? false)))
                continue;

            list.Add(item!);
        }
        return list;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}