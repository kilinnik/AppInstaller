using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AppInstaller.Converters;

public class ProgressBarGradientConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [double, string])
            return null;

        var progressValue = (double)values[0];
        var currentTheme = (string)values[1];
        var gradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5)
        };

        switch (currentTheme)
        {
            case "LightStandard":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#70AFF8"), 1)
                        );
                        break;
                }

                break;
            case "DarkStandard":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#417EC4"), 1)
                        );
                        break;
                }

                break;
            case "LightClassic":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#B0B0B0"), 1)
                        );
                        break;
                }

                break;
            case "DarkClassic":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#1A1A1A"), 1)
                        );
                        break;
                }

                break;
            case "LightQwerty":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#D16868"), 1)
                        );
                        break;
                }

                break;
            case "DarkQwerty":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#B21F1F"), 1)
                        );
                        break;
                }

                break;
            case "LightMrMeGaBaN":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#504050"), 1)
                        );
                        break;
                }

                break;
            case "DarkMrMeGaBaN":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#0a010f"), 1)
                        );
                        break;
                }

                break;

            case "LightClave":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#FF6347"), 1)
                        );
                        break;
                }

                break;

            case "DarkClave":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#660000"), 1)
                        );
                        break;
                }

                break;
            case "LightFate":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#A9A9C8"), 1) // Светло-серый фиолетовый
                        );
                        break;
                }
                break;

            case "DarkFate":
                switch (progressValue)
                {
                    default:
                        gradient.GradientStops.Add(
                            new GradientStop((Color)ColorConverter.ConvertFromString("#000000"), 1)
                        );
                        break;
                }
                break;
        }

        return gradient;
    }

    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
