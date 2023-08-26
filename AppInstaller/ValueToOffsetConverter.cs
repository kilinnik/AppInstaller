using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AppInstaller;

public class ProgressBarGradientConverter : IMultiValueConverter
{
    // public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    // {
    //     throw new NotImplementedException();
    // }

    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2 || values[0] is not double || values[1] is not string)
            return null;

        var progressValue = (double)values[0];
        var currentTheme = (string)values[1];
        var gradient = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };


        switch (currentTheme)
        {
            case "Light":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6247AA"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6247AA"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#9F58BE"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6247AA"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#9F58BE"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#7F58BE"),
                            1));
                        break;
                }

                break;
            case "Dark":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3A235A"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3A235A"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#5C3A8F"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3A235A"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#5C3A8F"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#4C3A8F"),
                            1));
                        break;
                }

                break;
        }

        return gradient;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}