using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AppInstaller.Converters;

public class ThemeToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string theme && parameter is string radioButtonName)
        {
            return theme == radioButtonName ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}