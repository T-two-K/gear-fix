using System.Globalization;
using System.Windows.Data;

namespace GearFix.Converters
{
    public class CardWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalWidth && totalWidth > 0)
            {
                int columns = totalWidth switch
                {
                    >= 800 => 3,
                    < 800 and > 550 => 2,
                    < 500 => 1,
                    _ => 1
                };

                double margin = 6 * 2;
                double padding = 12 * 2;
                double available = totalWidth - padding;
                return Math.Floor((available - margin * columns) / columns);
            }

            return 220.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
