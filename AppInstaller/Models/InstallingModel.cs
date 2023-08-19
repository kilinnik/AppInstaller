using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using SevenZip;

namespace AppInstaller.Models;

public class InstallingModel
{
    public event Action<int> ProgressChanged;
    public event Action<string, string, MessageBoxButton, MessageBoxImage> ErrorMessageOccurred;
    public event Action<string, string, bool> TimeChanged;

    private DateTime _lastUpdateTime = DateTime.MinValue;

    private readonly TimerModel _timerModel;
    private Func<Func<ArchiveFileInfo, decimal>, decimal> _sum;

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

    public async Task InstallApp(string archiveFolderPath, string? destinationFolderPath,
        string appName, string appVersion, bool iconChecked, IEnumerable<Components> components, List<string> exePaths,
        string size)
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

                await Task.Run(() =>
                    DecompressWithComponents(destinationFolderPath, archiveFolderPath, appName, components, size));

                if (iconChecked)
                {
                    CreateShortCut(destinationFolderPath, exePaths);
                }

                var uninstallBatPath = CreateUninstallBat(exePaths, destinationFolderPath, appName);
                AppRegistration(appName, appVersion, destinationFolderPath + exePaths[0], uninstallBatPath);
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
                $"An error occurred in InstallingModel.DecompressWithComponents(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            ErrorMessageOccurred(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _timerModel.Stop();
            _timerModel.Reset();
        }
    }

    private static void LoadEmbeddedSevenZipLibrary()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "AppInstaller.7z.dll";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new Exception("Resource not found!");
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);

        var tempDllPath = Path.Combine(Path.GetTempPath(), "7z.dll");

        // Загрузите конфигурационный файл
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        // Замените значение ключа или добавьте его, если он не существует
        config.AppSettings.Settings.Remove("7zLocation");
        config.AppSettings.Settings.Add("7zLocation", tempDllPath);

        // Сохраните изменения
        config.Save(ConfigurationSaveMode.Modified);

        // Обновите кэш настроек в текущем приложении
        ConfigurationManager.RefreshSection("appSettings");

        File.WriteAllBytes(tempDllPath, bytes);
        SevenZipBase.SetLibraryPath(tempDllPath);
    }

    private void DecompressWithComponents(string installPath, string archiveFolderPath, string appName,
        IEnumerable<Components> components, string appSize)
    {
        try
        {
            LoadEmbeddedSevenZipLibrary();

            // Получаем все архивы
            var archiveFiles = Directory.GetFiles(archiveFolderPath, "*.7z.001");

            // Вычисляем общий размер всех архивов
            var gigabytes = double.Parse(appSize[..^3]);
            var totalSize = (long)(gigabytes * 1024 * 1024 * 1024);
            var processedSize = 0L;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            void UpdateProgress(ulong size)
            {
                processedSize += (long)size;
                var progressPercentage = (int)((double)processedSize / totalSize * 100);
                if (progressPercentage > 100)
                {
                    processedSize = 100;
                }
                ProgressChanged(progressPercentage);
                
                var now = DateTime.Now;
                if (!((now - _lastUpdateTime).TotalSeconds >= 1)) return;
                _lastUpdateTime = now;

                var timePerByte = stopwatch.Elapsed.TotalSeconds / processedSize;
                var remainingSize = totalSize - processedSize;
                var remainingSeconds = timePerByte * remainingSize;
                var remainingTime = TimeSpan.FromSeconds(Math.Min(remainingSeconds, TimeSpan.MaxValue.TotalSeconds));
                var formattedRemainingTime = remainingTime.ToString(@"hh\:mm\:ss");
                TimeChanged("Времени осталось: ", formattedRemainingTime, false);
            }

            // Лямбда-обработчик события
            void FileExtractionFinishedHandler(object? _, FileInfoEventArgs e) => UpdateProgress(e.FileInfo.Size);

            // Распаковка основного приложения и компонентов
            void ExtractFiles(IEnumerable<string> filesToExtract)
            {
                foreach (var file in filesToExtract)
                {
                    using var extractor = new SevenZipExtractor(file);
                    extractor.FileExtractionFinished += FileExtractionFinishedHandler;
                    extractor.ExtractArchive(installPath);
                    extractor.FileExtractionFinished -= FileExtractionFinishedHandler;
                }
            }

            var mainArchiveFiles = archiveFiles.Where(file => file.Contains($"{appName}.7z.")).OrderBy(file => file)
                .ToList();
            ExtractFiles(mainArchiveFiles);

            foreach (var component in components.Where(c => c.IsChecked))
            {
                var componentArchiveFiles = archiveFiles.Where(file => file.Contains($"{component.FolderName}.7z."))
                    .OrderBy(file => file).ToList();
                ExtractFiles(componentArchiveFiles);
            }
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
        }
    }

    private static void CreateShortCut(string appFolder, List<string> exePaths)
    {
        foreach (var exePath in exePaths)
        {
            var link = (IShellLink)new ShellLink();

            // Настройка информации о ярлыке
            link.SetDescription("repack by nitokin");
            link.SetPath(appFolder + exePath);

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
        var pathToBat = $@"{pathToApp}\uninstall{appName}.bat";
        //var newFilePath = string.Empty;
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

            // // Получаем путь к папке appdata\local текущего пользователя
            // var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            //     "uninstalls");
            //
            // // Создаем директорию, если она не существует
            // Directory.CreateDirectory(appDataPath);
            // // Перемещаем файл
            // newFilePath = Path.Combine(appDataPath, Path.GetFileName(pathToBat));
            // if (File.Exists(newFilePath))
            // {
            //     File.Delete(newFilePath);
            // }
            //
            // File.Move(pathToBat, newFilePath);
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
        }

        return pathToBat;
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