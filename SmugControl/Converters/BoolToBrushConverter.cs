using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SmugControl.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        // value는 ServerStatus 타입이어야 함
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ServerStatus status)
            {
                switch (status)
                {
                    case ServerStatus.Normal:
                        return Brushes.SkyBlue;

                    case ServerStatus.Warning:
                        return Brushes.Gold;

                    case ServerStatus.Error:
                        return Brushes.Red;
                }
            }
            // 기본값
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum ServerStatus
    {
        Normal,
        Warning,
        Error
    }
}