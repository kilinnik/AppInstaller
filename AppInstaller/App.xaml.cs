using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using AppInstaller.ViewModels;
using AppInstaller.Views;
using Microsoft.Win32;

namespace AppInstaller;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            if (!CheckDotNetDesktopRuntime())
            {
                Shutdown();
                return;
            }

            var theme = ApplySystemTheme();

            base.OnStartup(e);

            var languageSelectionWindow = new LanguageSelectionWindow();
            if (languageSelectionWindow.ShowDialog() == true)
            {
                var selectedCulture =
                    languageSelectionWindow.SelectedCulture; //"ru-RU"; //languageSelectionWindow.SelectedCulture

                if (selectedCulture != null)
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedCulture);
                var mainWindowViewModel = new MainWindowViewModel(theme);
                var mainWindow = new MainWindow { DataContext = mainWindowViewModel };

                var filePath = GetConfigFilePath();
                if (File.Exists(filePath))
                {
                    LoadIcon(filePath);
                }

                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"An error occurred in App.OnStartup(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage +=
                    $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public void ToggleTheme(string theme)
    {
        var currentTheme = Resources.MergedDictionaries.FirstOrDefault(
            m =>
                m.Source.OriginalString.Contains("Themes/LightThemeStandard.xaml")
                || m.Source.OriginalString.Contains("Themes/DarkThemeStandard.xaml")
                || m.Source.OriginalString.Contains("Themes/LightThemeClassic.xaml")
                || m.Source.OriginalString.Contains("Themes/DarkThemeClassic.xaml")
                || m.Source.OriginalString.Contains("Themes/LightThemeQwerty.xaml")
                || m.Source.OriginalString.Contains("Themes/DarkThemeQwerty.xaml")
                || m.Source.OriginalString.Contains("Themes/LightThemeMrMeGaBaN.xaml")
                || m.Source.OriginalString.Contains("Themes/DarkThemeMrMeGaBaN.xaml")
                || m.Source.OriginalString.Contains("Themes/LightThemeClave.xaml")
                || m.Source.OriginalString.Contains("Themes/DarkThemeClave.xaml")
                || m.Source.OriginalString.Contains("Themes/LightThemeFate.xaml")
                || m.Source.OriginalString.Contains("Themes/DarkThemeFate.xaml")
        );

        if (currentTheme == null)
            return;

        var newTheme = currentTheme.Source.OriginalString.Contains("Light")
            ? new Uri($"Themes/DarkTheme{theme}.xaml", UriKind.Relative)
            : new Uri($"Themes/LightTheme{theme}.xaml", UriKind.Relative);

        Resources.MergedDictionaries.Remove(currentTheme);
        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = newTheme });
    }

    private static void LoadIcon(string filePath)
    {
        try
        {
            string? result = null;
            using (var reader = new StreamReader(filePath))
            {
                while (reader.ReadLine() is { } line)
                {
                    if (!line.StartsWith("RepackIcon="))
                        continue;
                    result = line["RepackIcon=".Length..].TrimEnd('\r', '\n');
                    break;
                }
            }

            if (result == null)
                return;

            var iconBytes = Convert.FromBase64String(result);

            using var stream = new MemoryStream(iconBytes);
            var iconImageSource = BitmapFrame.Create(
                stream,
                BitmapCreateOptions.None,
                BitmapCacheOption.OnLoad
            );

            if (Current.MainWindow != null)
            {
                Current.MainWindow.Icon = iconImageSource;
            }
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"An error occurred in App.LoadIcon(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage +=
                    $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string GetConfigFilePath()
    {
        var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "config_*.txt");
        var configFilePath = files.FirstOrDefault();

        if (configFilePath == null)
        {
            throw new FileNotFoundException("Config file not found.");
        }

        return configFilePath;
    }

    private static bool CheckDotNetDesktopRuntime()
    {
        const string runtimeVersion = "8.0";
        const string downloadLink =
            "https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime";

        try
        {
            var currentRuntime = System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-runtimes",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            );

            if (currentRuntime == null)
            {
                throw new InvalidOperationException("Unable to start dotnet process.");
            }

            var output = currentRuntime.StandardOutput.ReadToEnd();
            currentRuntime.WaitForExit();

            if (output.Contains($"Microsoft.WindowsDesktop.App {runtimeVersion}"))
            {
                return true;
            }

            MessageBox.Show(
                $"Required .NET Desktop Runtime {runtimeVersion} is not installed. Please download and install it from:\n{downloadLink}",
                "Missing .NET Desktop Runtime",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = downloadLink,
                    UseShellExecute = true
                }
            );

            return false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred while checking .NET Desktop Runtime: {ex.Message}\n{ex.StackTrace}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return false;
        }
    }

    private string ApplySystemTheme()
    {
        var isDarkMode = IsDarkTheme();
        var themeUri = isDarkMode
            ? new Uri("Themes/DarkThemeStandard.xaml", UriKind.Relative)
            : new Uri("Themes/LightThemeStandard.xaml", UriKind.Relative);

        var currentTheme = Resources.MergedDictionaries.FirstOrDefault(
            m =>
                m.Source?.OriginalString.Contains("Themes/LightThemeStandard.xaml") == true
                || m.Source?.OriginalString.Contains("Themes/DarkThemeStandard.xaml") == true
        );

        if (currentTheme != null)
        {
            Resources.MergedDictionaries.Remove(currentTheme);
        }

        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });

        return isDarkMode ? "DarkStandard" : "LightStandard";
    }


    private static bool IsDarkTheme()
    {
        const string key = "AppsUseLightTheme";
        const string subKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        var registryValue = Registry.CurrentUser.OpenSubKey(subKey)?.GetValue(key);

        return registryValue != null && (int)registryValue != 1;
    }
}