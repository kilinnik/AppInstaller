using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AppInstaller.Resources;
using Microsoft.Win32;

namespace AppInstaller.Models;

public class InstallingModel
{
    public event Action<int>? ProgressChanged;
    public event Action<string, string, MessageBoxButton, MessageBoxImage>? ErrorMessageOccurred;
    public event Action<string, string, bool>? TimeChanged;

    private readonly TimerModel _timerModel;
    private string _sevenZipPath;

    private readonly CancellationTokenSource _cts = new();

    public InstallingModel(TimerModel timerModel)
    {
        _timerModel = timerModel;
        _sevenZipPath = @"C:\Program Files\7-Zip\7z.exe";
    }

    private string FormatElapsedTime()
    {
        var elapsedSeconds = _timerModel.ElapsedSeconds;
        var hours = elapsedSeconds / 3600;
        var minutes = (elapsedSeconds % 3600) / 60;
        var seconds = elapsedSeconds % 60;
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    public async Task InstallApp(string archiveFolderPath, string? installPath, string appName,
        string appVersion, bool iconChecked, IEnumerable<Components> components, List<string> exePaths)
    {
        try
        {
            _timerModel.Start();
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (_, _) => TimeChanged($"{Strings.TimeElapsed} ", FormatElapsedTime(), true);
            timer.Start();
            if (installPath != null)
            {
                if (!Directory.Exists(installPath))
                {
                    Directory.CreateDirectory(installPath);
                }

                foreach (var file in Directory.EnumerateFiles(installPath, "*", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }

                await Task.Run(() => DecompressApp(installPath, archiveFolderPath, appName, components));

                if (iconChecked)
                {
                    CreateShortCut(installPath, exePaths);
                }

                var uninstallBatPath = CreateUninstallBat(exePaths, installPath, appName);
                AppRegistration(appName, appVersion, installPath + exePaths[0], uninstallBatPath);
            }

            var configFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.dll.config");
            foreach (var configFile in configFiles)
            {
                File.Delete(configFile);
            }

            _timerModel.Stop();
            _timerModel.Reset();
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"An error occurred in InstallingModel.InstallApp(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            ErrorMessageOccurred?.Invoke(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _timerModel.Stop();
            _timerModel.Reset();
        }
    }

    private static string? Get7ZipPathFromUser()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = $"{Strings.SevenZipExecutable} (7z.exe)|7z.exe",
            Title = Strings.TitleSelectExecutable,
        };
        
        return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
    }

    private async Task DecompressApp(string installPath, string archiveFolderPath, string appName,
        IEnumerable<Components> components)
    {
        try
        {
            var flag = !File.Exists(_sevenZipPath);
            if (flag)
            {
                await Install7ZipAsync();
            }
            
            if (!File.Exists(_sevenZipPath))
            {
                _sevenZipPath = Get7ZipPathFromUser() ?? throw new Exception(Strings.PathNotProvided);
                flag = false;
            }

            var mainArchivePath = File.Exists($"{archiveFolderPath}\\{appName}.7z.001")
                ? $"{archiveFolderPath}\\{appName}.7z.001"
                : $"{archiveFolderPath}\\{appName}.7z";

            await DecompressArchiveWith7Zip(installPath, mainArchivePath);

            foreach (var component in components.Where(c => c.IsChecked))
            {
                var componentArchivePath = File.Exists($"{archiveFolderPath}\\{component.FolderName}.7z.001")
                    ? $"{archiveFolderPath}\\{component.FolderName}.7z.001"
                    : $"{archiveFolderPath}\\{component.FolderName}.7z";

                await DecompressArchiveWith7Zip(installPath, componentArchivePath);
            }

            if (flag)
            {
                await Delete7ZipAsync();
            }
        }
        catch (Exception ex)
        {
            var errorMessage =
                @"Если установлен 7zip, то он обязательно должен находиться в папке C:\Program Files\7-Zip. " +
                $"An error occurred in InstallingModel.DecompressApp(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            ErrorMessageOccurred?.Invoke(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task DecompressArchiveWith7Zip(string extractPath, string archivePath)
    {
        var args = $"x \"{archivePath}\" -o\"{extractPath}\" -aoa -mmt -bsp1";

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = _sevenZipPath,
            Arguments = args,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        process.EnableRaisingEvents = true;

        _cts.Token.Register(() =>
        {
            if (!process.HasExited)
                process.Kill();
        });

        process.Start();
        var outputReadTask = ReadOutputAsync(process.StandardOutput);

        await process.WaitForExitAsync();
        await outputReadTask;

        if (process.ExitCode != 0)
        {
            var errorMessage = $"7-Zip process exited with code {process.ExitCode}";
            ErrorMessageOccurred?.Invoke(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ReadOutputAsync(TextReader reader)
    {
        var stopwatch = Stopwatch.StartNew();
        var nextUpdatePercent = 1;

        while (await reader.ReadLineAsync() is { } line)
        {
            var match = Regex.Match(line, @"(\d+)%");
            if (!match.Success) continue;
            var progressPercentage = int.Parse(match.Groups[1].Value);

            if (progressPercentage > 100) progressPercentage = 100;
            if (progressPercentage > nextUpdatePercent)
            {
                ProgressChanged?.Invoke(progressPercentage);
                nextUpdatePercent = progressPercentage;
            }

            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            var remainingTime = TimeSpan.FromSeconds(Math.Min(
                elapsedSeconds * (100 - progressPercentage) / progressPercentage, TimeSpan.MaxValue.TotalSeconds));
            var formattedRemainingTime = remainingTime.ToString(@"hh\:mm\:ss");

            TimeChanged($"{Strings.TimeRemaining} ", formattedRemainingTime, false);
        }
    }

    private async Task Delete7ZipAsync()
    {
        const string sevenZipUninstallPath = @"C:\Program Files\7-Zip\Uninstall.exe";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = sevenZipUninstallPath,
                Arguments = "/S",
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        await process.WaitForExitAsync(_cts.Token);
    }

    private async Task Install7ZipAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "AppInstaller.Resources.7z2301-x64.exe";
        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new Exception("Resource not found!");

        var tempInstallerPath = Path.Combine(Path.GetTempPath(), "7z2301-x64.exe");
        await using (var fileStream = new FileStream(tempInstallerPath, FileMode.Create, FileAccess.Write,
                         FileShare.None, 4096, true))
        {
            await stream.CopyToAsync(fileStream, 4096, _cts.Token);
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = tempInstallerPath,
                Arguments = "/S",
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        _cts.Token.Register(() =>
        {
            if (!process.HasExited)
                process.Kill();
        });

        process.Start();
        await process.WaitForExitAsync(_cts.Token);
    }

    private static void CreateShortCut(string appFolder, List<string> exePaths)
    {
        foreach (var exePath in exePaths)
        {
            var link = (IShellLink)new ShellLink();

            // Настройка информации о ярлыке
            link.SetDescription("repack by nitokin");
            link.SetPath(appFolder + exePath);
            link.SetWorkingDirectory(Path.GetDirectoryName(appFolder + exePath));

            // Сохранение
            var shortcut = (IPersistFile)link;
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var exeNameWithoutExtension = Path.GetFileNameWithoutExtension(exePath);
            shortcut.Save(Path.Combine(desktopPath, exeNameWithoutExtension + ".lnk"), false);
        }
    }

    private static void AppRegistration(string appName, string appVersion, string targetExePath,
        string uninstallBatPath)
    {
        using var uninstallKey =
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true);
        if (uninstallKey == null) return;
        using var appKey = uninstallKey.CreateSubKey(appName);
        appKey.SetValue("DisplayName", appName);
        appKey.SetValue("DisplayIcon", targetExePath);
        appKey.SetValue("DisplayVersion", appVersion);
        appKey.SetValue("Publisher", "Repack by Nitokin");
        appKey.SetValue("InstallLocation", Path.GetDirectoryName(targetExePath));
        appKey.SetValue("UninstallString", $"\"{uninstallBatPath}\" /uninstall");
    }

    private string CreateUninstallBat(IEnumerable<string> exePaths, string pathToApp, string appName)
    {
        var pathToBat = $@"{pathToApp}\uninstall.bat";
        try
        {
            // Читаем все строки из файла
            var lines = File.ReadAllLines(pathToBat).ToList();

            // Вставляем строки для удаления ярлыков на основе exePaths
            var indexForInsertion = 2;
            foreach (var exeNameWithoutExtension in exePaths.Select(Path.GetFileNameWithoutExtension))
            {
                lines.Insert(indexForInsertion, $"del \"%USERPROFILE%\\Desktop\\{exeNameWithoutExtension}.lnk\"");
                indexForInsertion++;
            }

            // Заменяем подстроки в каждой строке
            for (var i = indexForInsertion; i < lines.Count; i++)
            {
                lines[i] = lines[i].Replace("pathToApp", pathToApp);
                lines[i] = lines[i].Replace("appName", appName);
            }

            // Записываем обратно в файл
            File.WriteAllLines(pathToBat, lines);
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"An error occurred in InstallingModel.CreateUninstallBat(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            ErrorMessageOccurred?.Invoke(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return pathToBat;
    }

    public void Cancel()
    {
        _cts.Cancel();
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