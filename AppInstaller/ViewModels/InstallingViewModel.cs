using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AppInstaller.Models;
using ReactiveUI;

namespace AppInstaller.ViewModels;

public class InstallingViewModel : ViewModelBase
{
    private readonly InstallingModel _model;
    
    private string _currentTheme;
    public string CurrentTheme
    {
        get => _currentTheme;
        set => this.RaiseAndSetIfChanged(ref _currentTheme, value);
    }
    
    private MainWindowViewModel MainWindowViewModel { get; }

    private string _elapsedTime;

    public string ElapsedTime
    {
        get => _elapsedTime;
        set => this.RaiseAndSetIfChanged(ref _elapsedTime, value);
    }

    private string _remainingTime;

    public string RemainingTime
    {
        get => _remainingTime;
        set => this.RaiseAndSetIfChanged(ref _remainingTime, value);
    }

    private int _percentageProgress;

    public int PercentageProgress
    {
        get => _percentageProgress;
        set => this.RaiseAndSetIfChanged(ref _percentageProgress, value);
    }

    private double _progressValue;

    public double ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }

    private string _repackerName;

    public string RepackerName
    {
        get => _repackerName;
        set => this.RaiseAndSetIfChanged(ref _repackerName, value);
    }

    private ImageSource _headImage;

    public ImageSource HeadImage
    {
        get => _headImage;
        set => this.RaiseAndSetIfChanged(ref _headImage, value);
    }

    private ImageSource _mascotImage;

    public ImageSource MascotImage
    {
        get => _mascotImage;
        set => this.RaiseAndSetIfChanged(ref _mascotImage, value);
    }

    public InstallingViewModel(ImageSource headImage, MainWindowViewModel mainWindowViewModel, ImageSource mascotImage,
        string repackerName, InstallingModel model)
    {
        CurrentTheme = "Light";
        _model = model;
        _model.ProgressChanged += OnProgressChanged;
        _model.ErrorMessageOccurred += OnErrorMessageOccurred;
        _model.TimeChanged += OnTimeChanged;
        mainWindowViewModel.ExitRequested += () => model.Cancel();

        HeadImage = headImage;
        MainWindowViewModel = mainWindowViewModel;
        ElapsedTime = $"{Resources.Strings.TimeElapsed} 00:00:00";
        RemainingTime = $"{Resources.Strings.TimeRemaining} 00:00:00";
        MascotImage = mascotImage;
        RepackerName = "by " + repackerName;
    }

    public async Task InstallApp(string archiveFolderPath, string? destinationFolderPath,
        string appName, string appVersion, bool iconChecked, IEnumerable<Components> components, List<string> exePaths)
    {
        await _model.InstallApp(archiveFolderPath, destinationFolderPath, appName, appVersion, iconChecked,
            components, exePaths);
        MainWindowViewModel.NavigateNextView();
    }
    
    private void OnTimeChanged(string message, string time, bool flag)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (flag) ElapsedTime = message + time;
            else RemainingTime = message + time;
        });
    }

    private void OnProgressChanged(int value)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            PercentageProgress = value;
            ProgressValue = value;
        });
    }

    private static void OnErrorMessageOccurred(string message, string caption, MessageBoxButton buttons,
        MessageBoxImage icon)
    {
        MessageBox.Show(message, caption, buttons, icon);
    }
}