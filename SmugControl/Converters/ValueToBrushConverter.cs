using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SmugControl.Converters
{
    public class ValueToBrushConverter : IValueConverter
    {
        public Brush DefaultBrush { get; set; }
        public Brush WarningBrush { get; set; }
        public Brush DangerBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double val)
            {
                if (val >= 80) return DangerBrush;
                if (val >= 60) return WarningBrush;
                return DefaultBrush;
            }
            return DefaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}