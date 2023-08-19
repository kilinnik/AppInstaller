using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppInstaller.Models;

public class MainWindowModel
{
    public event Action<string, string, MessageBoxButton, MessageBoxImage> ErrorMessageOccurred;

    public string SelectedPath { get; set; }
    public bool IconChecked { get; set; }
    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");

    public MainWindowModel()
    {
        SelectedPath = $@"C:\Program Files (x86)\{GetAppName()}";
    }

    private BitmapImage GetImageFromConfig(string key)
    {
        var bitmapImage = new BitmapImage();
        try
        {
            if (!File.Exists(_filePath)) throw new FileNotFoundException("Config file not found.");

            var content = File.ReadAllText(_filePath);
            var startIndex = content.IndexOf($"{key}=", StringComparison.Ordinal) + $"{key}=".Length;
            var endIndex = content.IndexOf('\n', startIndex);
            var result = content.Substring(startIndex, endIndex - startIndex);

            var imageBytes = Convert.FromBase64String(result);

            using var stream = new MemoryStream(imageBytes);
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"An error occurred in InstallingModel.DecompressWithComponents(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            ErrorMessageOccurred(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }

        return bitmapImage;
    }

    public ImageSource GetMascotImage() => GetImageFromConfig("MascotImage");
    public ImageSource GetBigImage() => GetImageFromConfig("AppBigImage");
    public ImageSource GetHeadImage() => GetImageFromConfig("AppHeadImage");

    
    public string GetNeededMemory() => GetValueFromConfig("AppSize", "RepackDescription");
    public string GetRepackDescription() => GetValueFromConfig("RepackDescription", "AppPurchaseLink");
    public string GetAppPurchaseLink() => GetValueFromConfig("AppPurchaseLink", "RepackerName");
    public string GetRepackerName() => GetValueFromConfig("RepackerName", "AppVersion");
    public string GetAppVersion() => GetValueFromConfig("AppVersion", "AppName");
    public string GetAppName() => GetValueFromConfig("AppName", "AppDescription");
    public string GetAppDescription() => GetValueFromConfig("AppDescription", "RepackIcon");

    private string GetValueFromConfig(string startTag, string endTag)
    {
        try
        {
            if (!File.Exists(_filePath)) throw new FileNotFoundException("Config file not found.");
            var content = File.ReadAllText(_filePath);
            var startIndex = content.IndexOf($"{startTag}=", StringComparison.Ordinal) + $"{startTag}=".Length;
            var endIndex = content.IndexOf($"\r\n{endTag}", startIndex, StringComparison.Ordinal);
            return content.Substring(startIndex, endIndex - startIndex);
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"An error occurred in InstallingModel.DecompressWithComponents(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            ErrorMessageOccurred(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    public IEnumerable<KeyValuePair<string, string>> GetComponentNames()
    {
        try
        {
            if (!File.Exists(_filePath)) throw new FileNotFoundException("Config file not found.");
            var lines = File.ReadLines(_filePath);

            return (from line in lines.TakeWhile(line => !line.StartsWith("AppExePath"))
                select line.Split('=')
                into parts
                where parts.Length == 2
                let folderName = parts[0].Trim()
                let componentName = parts[1].Trim()
                select new KeyValuePair<string, string>(folderName, componentName)).ToList();
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"An error occurred in InstallingModel.DecompressWithComponents(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            ErrorMessageOccurred(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }
    
    public List<string> GetExePaths()
    {
        try
        {
            if (!File.Exists(_filePath)) throw new FileNotFoundException("Config file not found.");
            
            var paths = new List<string>();

            using var reader = new StreamReader(_filePath);
            while (reader.ReadLine() is { } line)
            {
                if (!line.StartsWith("AppExePath")) continue;
                var parts = line.Split('=');
                if (parts.Length > 1)
                {
                    paths.Add(parts[1]);
                }
            }

            return paths;
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"An error occurred in InstallingModel.DecompressWithComponents(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            ErrorMessageOccurred(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }
}