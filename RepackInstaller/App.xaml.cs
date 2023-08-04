using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using RepackInstaller.ViewModels;
using RepackInstaller.Views;

namespace RepackInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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
                        result = line["RepackIcon=".Length..];
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
                MessageBox.Show($"An error occurred in App.LoadIcon(): {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}