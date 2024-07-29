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
        if (values.Length != 2 || values[0] is not double || values[1] is not string)
            return null;

        var progressValue = (double)values[0];
        var currentTheme = (string)values[1];
        var gradient = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };


        switch (currentTheme)
        {
            case "LightStandard":
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
            case "DarkStandard":
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
            case "LightClassic":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFF"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFF"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#E0E0E0"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFF"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#E0E0E0"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#B0B0B0"),
                            1));
                        break;
                }

                break;
            case "DarkClassic":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#4D4D4D"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#4D4D4D"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#2E2E2E"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#4D4D4D"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#2E2E2E"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1A1A1A"),
                            1));
                        break;
                }

                break;
            case "LightLivingsamurai":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#53446A"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#53446A"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#66557D"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#53446A"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#66557D"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#736291"),
                            1));
                        break;
                }

                break;
            case "DarkLivingsamurai":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#251C3D"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#251C3D"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3D2852"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#251C3D"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3D2852"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#453166"),
                            1));
                        break;
                }

                break;
            case "LightTemplarFulga":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6A7AB0"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6A7AB0"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#4A5A90"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6A7AB0"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#4A5A90"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3A4A80"),
                            1));
                        break;
                }

                break;
            case "DarkTemplarFulga":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#191970"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#191970"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#000080"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#191970"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#000080"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#082567"),
                            1));
                        break;
                }

                break;
            case "LightQwerty":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#5A7CA0"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#5A7CA0"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#B58A90"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#5A7CA0"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#B58A90"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#D16868"),
                            1));
                        break;
                }

                break;
            case "DarkQwerty":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1A2A6C"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1A2A6C"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#9A1E1E"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1A2A6C"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#9A1E1E"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#B21F1F"),
                            1));
                        break;
                }

                break;
            case "LightMrMeGaBaN":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6E8CA0"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6E8CA0"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#8F6E9C"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6E8CA0"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#8F6E9C"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#504050"),
                            1));
                        break;
                }

                break;
            case "DarkMrMeGaBaN":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#162046"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#162046"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3D2852"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#162046"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3D2852"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#0a010f"),
                            1));
                        break;
                }

                break;
            case "LightGrustyck":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#058960"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#058960"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#21A982"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#058960"),
                            0.3333));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#21A982"),
                            0.6666));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#37CBA7"),
                            1));
                        break;
                }

                break;
            case "DarkGrustyck":
                switch (progressValue)
                {
                    case <= 33:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#003333"),
                            1));
                        break;
                    case <= 66:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#003333"),
                            0.5));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#005555"),
                            1));
                        break;
                    default:
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#003333"),
                            0.5));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#005555"),
                            1));
                        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#007777"),
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