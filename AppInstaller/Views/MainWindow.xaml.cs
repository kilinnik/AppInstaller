using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using AppInstaller.ViewModels;
using NAudio.Wave;

namespace AppInstaller.Views;

public partial class MainWindow
{
    private const string FilePattern = "config_*.txt";
    private static readonly string DirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string FilePath = FindConfigFile();

    private readonly Dictionary<string, string> _themeToggles =
        new()
        {
            { "LightStandard", "DarkStandard" },
            { "DarkStandard", "LightStandard" },
            { "LightClassic", "DarkClassic" },
            { "DarkClassic", "LightClassic" },
            { "LightLivingsamurai", "DarkLivingsamurai" },
            { "DarkLivingsamurai", "LightLivingsamurai" },
            { "LightTemplarFulga", "DarkTemplarFulga" },
            { "DarkTemplarFulga", "LightTemplarFulga" },
            { "LightQwerty", "DarkQwerty" },
            { "DarkQwerty", "LightQwerty" },
            { "LightMrMeGaBaN", "DarkMrMeGaBaN" },
            { "DarkMrMeGaBaN", "LightMrMeGaBaN" },
            { "LightGrustyck", "DarkGrustyck" },
            { "DarkGrustyck", "LightGrustyck" },
            { "LightClave", "DarkClave" },
            { "DarkClave", "LightClave" },
            { "LightFate", "DarkFate" },
            { "DarkFate", "LightFate" }
        };

    private bool IsPlaying { get; set; }

    private WaveOutEvent _waveOut;

    private Mp3FileReader _mp3Reader;

    public MainWindow()
    {
        InitializeComponent();
        Closing += OnWindowClosing;

        var track = GetTrack();

        if (WaveOut.DeviceCount > 0)
        {
            _waveOut = new WaveOutEvent();
            _waveOut.PlaybackStopped += OnPlaybackStopped;

            var ms = new MemoryStream(track);
            _mp3Reader = new Mp3FileReader(ms);
            _waveOut.Init(_mp3Reader);
        }
        else
        {
            MessageBox.Show(
                "Аудиоустройства не обнаружены. Воспроизведение отключено.",
                "Внимание",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }
    }
    
    private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _waveOut.Stop();
        _waveOut.Dispose();
        _mp3Reader.Dispose();
        Application.Current.Shutdown(); 
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            MessageBox.Show($"Ошибка при воспроизведении: {e.Exception.Message}");
            return;
        }

        if (!IsPlaying)
            return;
        _mp3Reader.Position = 0;
        _waveOut.Play();
    }

    private void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (!IsPlaying)
        {
            _waveOut.Play();
            PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            IsPlaying = true;
        }
        else
        {
            _waveOut.Pause();
            PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            IsPlaying = false;
        }
    }

    private void ResetTrack_Click(object sender, RoutedEventArgs e)
    {
        _mp3Reader.Position = 0;
        if (IsPlaying)
            return;
        _waveOut.Play();
        IsPlaying = true;
    }

    private static byte[] GetTrack()
    {
        var result = string.Empty;
        try
        {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException("Config file not found.");

            var content = File.ReadAllText(FilePath);
            var startIndex = content.IndexOf("Track=", StringComparison.Ordinal) + "Track=".Length;
            var endIndex = content.IndexOf('\n', startIndex);
            if (startIndex > 5)
            {
                result = content.Substring(startIndex, endIndex - startIndex);
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();

                using var stream = assembly.GetManifestResourceStream(
                    "AppInstaller.Resources.default_track.txt"
                );
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    result = reader.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"An error occurred in MainWindow.GetTrack(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage +=
                    $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }

        return Convert.FromBase64String(result);
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
    }

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
        var app = (App)Application.Current;
        var viewModel = (MainWindowViewModel)DataContext;
        if (viewModel is null)
            return;

        var themeName = (sender as RadioButton)?.Name;
        if (string.IsNullOrEmpty(themeName))
            return;

        var isLightTheme = viewModel.CurrentTheme.Contains("Light");
        var fullThemeName = isLightTheme ? $"Light{themeName}" : $"Dark{themeName}";

        app.ChangeTheme(themeName);
        viewModel.CurrentTheme = fullThemeName;
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        ThemePopup.IsOpen = true;
    }

    private static string FindConfigFile()
    {
        var files = Directory.GetFiles(DirectoryPath, FilePattern);
        return files.FirstOrDefault() ?? throw new FileNotFoundException();
    }
}
