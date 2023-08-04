using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RepackInstaller;

public class ProgressBarGradientConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var progressValue = (double)value;
        var gradient = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };

        switch (progressValue)
        {
            case <= 33:
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6247AA"), 1));
                break;
            case <= 66:
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6247AA"), 0.3333));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#9F58BE"), 1));
                break;
            default:
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6247AA"), 0.3333));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#9F58BE"), 0.6666));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#7F58BE"), 1));
                break;
        }

        return gradient;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}