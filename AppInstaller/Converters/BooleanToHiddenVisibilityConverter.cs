using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AppInstaller.Converters;

public class BooleanToHiddenVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed; // Изменено с Hidden на Collapsed
        }

        return Visibility.Collapsed; // Изменено с Hidden на Collapsed
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility visibilityValue)
        {
            return visibilityValue == Visibility.Visible;
        }

        return false;
    }
}