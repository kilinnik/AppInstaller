using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace RepackInstaller.Models;

public class InstallingModel
{
    public event Action<int> ProgressChanged;
    public event Action<string, string, MessageBoxButton, MessageBoxImage> ErrorMessageOccurred;
    public event Action<string, string, bool> TimeChanged;

    private DateTime _lastUpdateTime = DateTime.MinValue;

    private readonly TimerModel _timerModel;

    public InstallingModel(TimerModel timerModel)
    {
        _timerModel = timerModel;
    }

    private string FormatElapsedTime()
    {
        var elapsedSeconds = _timerModel.ElapsedSeconds;
        var hours = elapsedSeconds / 3600;
        var minutes = (elapsedSeconds % 3600) / 60;
        var seconds = elapsedSeconds % 60;
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    public async Task InstallGame(string inputPath, string? destinationFolderPath, string targetExePath,
        string gameName, string gameVersion, bool iconChecked)
    {
        try
        {
            _timerModel.Start();
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (_, _) => TimeChanged("Времени прошло: ", FormatElapsedTime(), true);
            timer.Start();
            if (destinationFolderPath != null)
            {
                if (!Directory.Exists(destinationFolderPath))
                {
                    Directory.CreateDirectory(destinationFolderPath);
                }

                foreach (var file in Directory.EnumerateFiles(destinationFolderPath, "*", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }

                await Task.Run(() => DecompressDirectoryWithProgress(inputPath, destinationFolderPath));

                if (iconChecked)
                {
                    CreateShortCut(targetExePath, gameName);
                }

                var uninstallBatPath = CreateUninstallBat(gameName, destinationFolderPath);
                GameRegistration(gameName, gameVersion, targetExePath, uninstallBatPath);
            }

            _timerModel.Stop();
            _timerModel.Reset();
        }
        catch (Exception ex)
        {
            ErrorMessageOccurred($"An error occurred in InstallingModel.InstallGame(): {ex.Message}", "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            _timerModel.Stop();
            _timerModel.Reset();
        }
    }

    private void DecompressDirectoryWithProgress(string zipPath, string extractPath)
    {
        try
        {
            using var zipToOpen = new FileStream(zipPath, FileMode.Open);
            using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);
            var totalSize = archive.Entries.Sum(entry => entry.Length);
            long processedSize = 0;
            var stopwatch = Stopwatch.StartNew();

            foreach (var entry in archive.Entries)
            {
                var fullPath = Path.Combine(extractPath, entry.FullName);
                var directoryPath = Path.GetDirectoryName(fullPath);

                // Создаем папки при необходимости
                if (directoryPath != null) Directory.CreateDirectory(directoryPath);

                // Распаковываем файлы
                using var fileStream = File.Create(fullPath);
                using var zipStream = entry.Open();
                var buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                    processedSize += bytesRead;

                    var progressPercentage = (int)((double)processedSize / totalSize * 100);
                    ProgressChanged?.Invoke(progressPercentage);

                    var now = DateTime.Now;
                    if (!((now - _lastUpdateTime).TotalSeconds >= 1)) continue;
                    _lastUpdateTime = now;
                    var timePerByte = stopwatch.Elapsed.TotalSeconds / processedSize;
                    var remainingSize = totalSize - processedSize;
                    var remainingTime = TimeSpan.FromSeconds(timePerByte * remainingSize);
                    var formattedRemainingTime = remainingTime.ToString(@"hh\:mm\:ss");

                    TimeChanged?.Invoke("Времени осталось: ", formattedRemainingTime, false);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessageOccurred($"An error occurred in InstallingModel.DecompressDirectoryWithProgress(): {ex.Message}", "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static void CreateShortCut(string targetExePath, string? gameName)
    {
        var link = (IShellLink)new ShellLink();

        // setup shortcut information
        link.SetDescription("repack by nitokin");
        link.SetPath(targetExePath);

        // save it
        var shortcut = (IPersistFile)link;
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        shortcut.Save(Path.Combine(desktopPath, gameName + ".lnk"), false);
    }

    private static void GameRegistration(string gameName, string gameVersion, string targetExePath,
        string uninstallBatPath)
    {
        using var uninstallKey =
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true);
        if (uninstallKey == null) return;
        using var appKey = uninstallKey.CreateSubKey(gameName);
        appKey.SetValue("DisplayName", gameName);
        appKey.SetValue("DisplayIcon", targetExePath);
        appKey.SetValue("DisplayVersion", gameVersion);
        appKey.SetValue("Publisher", "Repack by Nitokin");
        appKey.SetValue("InstallLocation", Path.GetDirectoryName(targetExePath));
        appKey.SetValue("UninstallString", $"\"{uninstallBatPath}\" /uninstall");
    }

    private string CreateUninstallBat(string? gameName, string pathToApp)
    {
        var pathToBat = $@"{pathToApp}\uninstall{gameName}.bat";
        var newFilePath = string.Empty;
        try
        {
            // Читаем все строки из файла
            var lines = File.ReadAllLines(pathToBat);

            // Заменяем подстроки в каждой строке
            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Replace("gameName", gameName);
                lines[i] = lines[i].Replace("pathToApp", pathToApp);
            }

            // Записываем обратно в файл
            File.WriteAllLines(pathToBat, lines);

            // Получаем путь к папке appdata\local текущего пользователя
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "uninstalls");

            // Создаем директорию, если она не существует
            Directory.CreateDirectory(appDataPath);
            // Перемещаем файл
            newFilePath = Path.Combine(appDataPath, Path.GetFileName(pathToBat));
            if (File.Exists(newFilePath))
            {
                File.Delete(newFilePath);
            }

            File.Move(pathToBat, newFilePath);
        }
        catch (Exception ex)
        {
            ErrorMessageOccurred($"An error occurred in InstallingModel.CreateUninstall(): {ex.Message}", "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        return newFilePath;
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    private class ShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd,
            int fFlags);

        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);

        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath,
            out int piIcon);

        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
}