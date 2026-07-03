using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GearFix.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is IEnumerable<object> list)
            {
                if (list.Any())
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
