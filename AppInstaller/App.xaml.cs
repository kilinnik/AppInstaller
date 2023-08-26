using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using AppInstaller.ViewModels;
using AppInstaller.Views;

namespace AppInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                var languageSelectionWindow = new LanguageSelectionWindow();
                if (languageSelectionWindow.ShowDialog() == true)
                {
                    SetTheme(new Uri("Themes/LightTheme.xaml", UriKind.Relative));
                    
                    var selectedCulture = languageSelectionWindow.SelectedCulture;
                    if (selectedCulture != null)
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedCulture);
                    var mainWindowViewModel = new MainWindowViewModel();
                    var mainWindow = new MainWindow
                    {
                        DataContext = mainWindowViewModel
                    };

                    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
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
                var errorMessage = $"An error occurred in App.LoadIcon(): {ex.Message}\n{ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
                }

                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetTheme(Uri theme)
        {
            var currentTheme = Resources.MergedDictionaries.FirstOrDefault(
                m => m.Source.OriginalString.Contains("Themes/LightTheme.xaml") ||
                     m.Source.OriginalString.Contains("Themes/DarkTheme.xaml"));

            if (currentTheme != null)
            {
                Resources.MergedDictionaries.Remove(currentTheme);
            }

            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = theme });
        }
        
        public void ToggleTheme()
        {
            var currentTheme = Resources.MergedDictionaries.FirstOrDefault(
                m => m.Source.OriginalString.Contains("Themes/LightTheme.xaml") ||
                     m.Source.OriginalString.Contains("Themes/DarkTheme.xaml"));

            if (currentTheme == null) return;
            var newTheme = currentTheme.Source.OriginalString.Contains("Themes/LightTheme.xaml")
                ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

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
                        if (!line.StartsWith("RepackIcon=")) continue;
                        result = line["RepackIcon=".Length..].TrimEnd('\r', '\n');
                        break;
                    }
                }

                if (result == null) return;

                var iconBytes = Convert.FromBase64String(result);

                using var stream = new MemoryStream(iconBytes);
                var iconImageSource = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                if (Current.MainWindow != null)
                {
                    Current.MainWindow.Icon = iconImageSource;
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred in App.LoadIcon(): {ex.Message}\n{ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
                }

                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}