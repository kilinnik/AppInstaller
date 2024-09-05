using System;
using System.Globalization;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace AppInstaller.Converters;

public class ViewIndexToIconKindConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            return index switch
            {
                2 => PackIconKind.ArrowDown,
                4 => PackIconKind.Check,
                _ => PackIconKind.ArrowRight
            };
        }

        return PackIconKind.ArrowRight;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
