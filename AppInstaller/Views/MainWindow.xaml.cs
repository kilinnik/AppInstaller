using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using AppInstaller.ViewModels;

namespace AppInstaller.Views;

public partial class MainWindow
{
    private readonly Dictionary<string, string> _themeToggles =
        new()
        {
            { "LightStandard", "DarkStandard" },
            { "DarkStandard", "LightStandard" },
            { "LightClassic", "DarkClassic" },
            { "DarkClassic", "LightClassic" },
            { "LightQwerty", "DarkQwerty" },
            { "DarkQwerty", "LightQwerty" },
            { "LightMrMeGaBaN", "DarkMrMeGaBaN" },
            { "DarkMrMeGaBaN", "LightMrMeGaBaN" },
            { "LightClave", "DarkClave" },
            { "DarkClave", "LightClave" },
            { "LightFate", "DarkFate" },
            { "DarkFate", "LightFate" }
        };

    public MainWindow()
    {
        InitializeComponent();
        Closing += OnWindowClosing;
    }

    private static void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = Application.Current.MainWindow;

        if (mainWindow != null)
            mainWindow.WindowState = WindowState.Minimized;
    }

    private void ToggleTheme(object sender, RoutedEventArgs e)
    {
        var app = (App)Application.Current;
        var viewModel = (MainWindowViewModel)DataContext;

        viewModel.CurrentTheme = _themeToggles.TryGetValue(viewModel.CurrentTheme, out var newTheme)
            ? newTheme
            : viewModel.CurrentTheme;

        var themeType = viewModel.CurrentTheme.Replace("Light", "").Replace("Dark", "");
        app.ToggleTheme(themeType);

        if (!viewModel.IsDefaultLogo || Application.Current.Resources["TextBrush"] is not SolidColorBrush textBrush ||
            AppPurchaseLinkImage == null) return;
        var newIconSource = viewModel.AppPurchaseLinkLogo;
        ApplyColorToImage(newIconSource, textBrush.Color);
        AppPurchaseLinkImage.Source = newIconSource;
    }


    private static void ApplyColorToImage(DrawingImage image, Color color)
    {
        ApplyColorToDrawing(image.Drawing, color);
    }

    private static void ApplyColorToDrawing(Drawing drawing, Color color)
    {
        switch (drawing)
        {
            case DrawingGroup drawingGroup:
            {
                foreach (var child in drawingGroup.Children)
                {
                    ApplyColorToDrawing(child, color);
                }
                break;
            }
            case GeometryDrawing geometryDrawing:
                geometryDrawing.Brush = new SolidColorBrush(color);
                break;
        }
    }
}
