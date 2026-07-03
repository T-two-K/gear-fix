using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace GearFix.Converters
{
    public class DangerToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int danger)
            {
                return danger switch
                {
                    <= 10 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71")),
                    <= 20 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5DBE6E")),
                    <= 30 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A8C83A")),
                    <= 40 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4C026")),
                    <= 50 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0A500")),
                    <= 60 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F07800")),
                    <= 70 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E85200")),
                    <= 80 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E03020")),
                    <= 90 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC1010")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A00000"))
                };
            }
            else
            {
                return new SolidColorBrush(Colors.Gray);
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
