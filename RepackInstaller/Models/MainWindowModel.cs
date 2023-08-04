using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RepackInstaller.Models;

public class MainWindowModel
{
    public event Action<string, string, MessageBoxButton, MessageBoxImage> ErrorMessageOccurred;

    public string SelectedPath { get; set; }
    public bool IconChecked { get; set; }
    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");

    public MainWindowModel()
    {
        SelectedPath = $@"C:\Program Files (x86)\{GetGameName()}";
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
            ErrorMessageOccurred($"An error occurred in MainWindowModel.GetImageFromConfig(): {ex.Message}", "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            throw;
        }

        return bitmapImage;
    }

    public ImageSource GetMascotImage() => GetImageFromConfig("MascotImage");
    public ImageSource GetBigImage() => GetImageFromConfig("GameBigImage");
    public ImageSource GetHeadImage() => GetImageFromConfig("GameHeadImage");

    public string GetGameName() => GetValueFromConfig("GameName", "GameDescription");
    public string GetSteamGameLink() => GetValueFromConfig("GameSteamLink", "RepackerName");
    public string GetGameVersion() => GetValueFromConfig("GameVersion", "GameName");
    public string GetGameExePath() => GetValueFromConfig("GameExePath", "GameSize");
    public string GetGameDescription() => GetValueFromConfig("GameDescription", "RepackIcon");
    public string GetNeededMemory() => GetValueFromConfig("GameSize", "RepackDescription");
    public string GetRepackerName() => GetValueFromConfig("RepackerName", "GameVersion");

    private string GetValueFromConfig(string startTag, string endTag)
    {
        try
        {
            if (!File.Exists(_filePath)) throw new FileNotFoundException("Config file not found.");
            var content = File.ReadAllText(_filePath);
            var startIndex = content.IndexOf($"{startTag}=", StringComparison.Ordinal) + $"{startTag}=".Length;
            var endIndex = content.IndexOf($"\n{endTag}", startIndex, StringComparison.Ordinal);
            return content.Substring(startIndex, endIndex - startIndex);
        }
        catch (Exception ex)
        {
            ErrorMessageOccurred($"An error occurred in MainWindowModel.GetValueFromConfig(): {ex.Message}", "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            throw;
        }
    }
}