using System.Globalization;
using System.Windows.Data;

namespace SmugControl.Converters;

public sealed class SelectedEqualsItemConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return false;
        var selected = values[0];
        var item = values[1];
        return ReferenceEquals(selected, item) || (selected?.Equals(item) ?? false);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}