﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppInstaller.Models;

public class MainWindowModel
{
    public event Action<string, string, MessageBoxButton, MessageBoxImage> ErrorMessageOccurred;

    public string SelectedPath { get; set; }

    public bool IconChecked { get; set; }

    private readonly string _filePath = GetConfigFilePath();

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
                $"An error occurred in MainWindowModel.GetImageFromConfig(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            ErrorMessageOccurred
                (errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }

        return bitmapImage;
    }

    public ImageSource GetMascotImage() => GetImageFromConfig("MascotImage");
    public ImageSource GetBigImage() => GetImageFromConfig("AppBigImage");
    public ImageSource GetHeadImage() => GetImageFromConfig("AppHeadImage");

    private static string GetCurrentCultureSuffix()
    {
        return Thread.CurrentThread.CurrentUICulture.Name switch
        {
            "ru-RU" => "Ru",
            "en-US" => "En",
            _ => "En"
        };
    }

    public string GetNeededMemory() => GetValueFromConfig("AppSize", "RepackDescription");

    public string GetRepackDescription()
    {
        var suffix = GetCurrentCultureSuffix();
        return GetValueFromConfig($"RepackDescription{suffix}",
            suffix == "En" ? "AppPurchaseLink" : "RepackDescriptionEn");
    }

    public string GetAppPurchaseLink() => GetValueFromConfig("AppPurchaseLink", "RepackerName");
    public string GetRepackerName() => GetValueFromConfig("RepackerName", "AppVersion");
    public string GetAppVersion() => GetValueFromConfig("AppVersion", "AppName");
    public string GetAppName() => GetValueFromConfig("AppName", "AppDescription");

    public string GetAppDescription()
    {
        var suffix = GetCurrentCultureSuffix();
        return GetValueFromConfig($"AppDescription{suffix}", suffix == "En" ? "AppTheme" : "AppDescriptionEn");
    }

    public string GetAppTheme() => GetValueFromConfig("AppTheme", "RepackIcon");

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
                $"An error occurred in MainWindowModel.GetValueFromConfig(): {ex.Message}\n{ex.StackTrace}";
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
            var prefix = GetCurrentCultureSuffix();

            return (from line in lines.TakeWhile(line => !line.StartsWith("AppExePath"))
                where line.StartsWith(prefix)
                select line.Split('=')
                into parts
                where parts.Length == 2
                let folderNameWithPrefix = parts[0].Trim()
                let componentName = parts[1].Trim()
                let folderName = ReplaceFirst(folderNameWithPrefix, prefix, "")
                select new KeyValuePair<string, string>(folderName, componentName)).ToList();
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"An error occurred in MainWindowModel.GetComponentNames(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            ErrorMessageOccurred(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private static string ReplaceFirst(string text, string search, string replace)
    {
        var pos = text.IndexOf(search, StringComparison.Ordinal);
        return pos < 0 ? text : string.Concat(text.AsSpan(0, pos), replace, text.AsSpan(pos + search.Length));
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
                $"An error occurred in MainWindowModel.GetExePaths(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            ErrorMessageOccurred(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
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
}