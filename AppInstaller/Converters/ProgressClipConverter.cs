using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AppInstaller.Converters;

public class LeftOnlyRoundedClipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double width)
        {
            // Создаем обрезку, которая закругляет только левый край
            return new RectangleGeometry(new Rect(0, 0, width, 30), 10, 10);
        }
        return new RectangleGeometry(new Rect(0, 0, 0, 0));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}