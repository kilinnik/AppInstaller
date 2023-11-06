using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using AppInstaller.Models;
using AppInstaller.Resources;
using AppInstaller.Views;
using ReactiveUI;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace AppInstaller.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly MainWindowModel _mainWindowModel;

    private int _currentViewIndex;

    private readonly UserControl?[] _views;

    private readonly string _appName;

    private readonly IEnumerable<KeyValuePair<string, string>> _components;

    public string SelectedPath
    {
        get => _mainWindowModel.SelectedPath;
        set => _mainWindowModel.SelectedPath = value;
    }

    public bool IconChecked
    {
        get => _mainWindowModel.IconChecked;
        set => _mainWindowModel.IconChecked = value;
    }

    private DrawingImage _appPurchaseLinkLogo;

    public DrawingImage AppPurchaseLinkLogo
    {
        get => _appPurchaseLinkLogo;
        set => this.RaiseAndSetIfChanged(ref _appPurchaseLinkLogo, value);
    }

    private string? _appTitleDisplay;

    public string? AppTitleDisplay
    {
        get => _appTitleDisplay;
        set => this.RaiseAndSetIfChanged(ref _appTitleDisplay, value);
    }

    private UserControl? _currentView;

    public UserControl? CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    private bool _isBackButtonVisible;

    public bool IsBackButtonVisible
    {
        get => _isBackButtonVisible;
        set => this.RaiseAndSetIfChanged(ref _isBackButtonVisible, value);
    }

    private bool _isNextButtonVisible;

    public bool IsNextButtonVisible
    {
        get => _isNextButtonVisible;
        set => this.RaiseAndSetIfChanged(ref _isNextButtonVisible, value);
    }

    private bool _isCancelButtonVisible;

    public bool IsCancelButtonVisible
    {
        get => _isCancelButtonVisible;
        set => this.RaiseAndSetIfChanged(ref _isCancelButtonVisible, value);
    }

    private string? _buttonNextText;

    public string? ButtonNextText
    {
        get => _buttonNextText;
        set => this.RaiseAndSetIfChanged(ref _buttonNextText, value);
    }
    
    private string? _appTheme;

    public string? AppTheme
    {
        get => _appTheme;
        set => this.RaiseAndSetIfChanged(ref _appTheme, value);
    }

    private string _currentTheme;

    public string CurrentTheme
    {
        get => _currentTheme;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentTheme, value);
            if (_views[3]?.DataContext is InstallingViewModel installingViewModel)
            {
                installingViewModel.CurrentTheme = value;
            }
        }
    }

    public ReactiveCommand<Unit, Unit> ShowMessageBoxCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowChatGptWindowCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenTgLinkCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenAppPurchaseLinkCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToNextViewCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToPreviousViewCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public event Action ExitRequested;

    public MainWindowViewModel()
    {
        _mainWindowModel = new MainWindowModel();
        _mainWindowModel.ErrorMessageOccurred += OnErrorMessageOccurred;
        _components = _mainWindowModel.GetComponentNames();
        _appName = _mainWindowModel.GetAppName();
        AppTheme = _mainWindowModel.GetAppTheme();
        AppTitleDisplay = $"R. G. NITOKIN - {_appName}";
        var bigImage = _mainWindowModel.GetBigImage();
        var headImage = _mainWindowModel.GetHeadImage();
        var link = _mainWindowModel.GetAppPurchaseLink();
        SetLogo(link);

        _views = new UserControl?[]
        {
            new WelcomeView
            {
                DataContext = new WelcomeViewModel(_appName, bigImage, _mainWindowModel.GetAppDescription())
            },
            new SelectDirView
            {
                DataContext = new SelectDirViewModel(this, headImage, _appName,
                    _mainWindowModel.GetNeededMemory(), _components)
            },
            new ReadyView { DataContext = new ReadyViewModel(headImage) },
            new InstallingView
            {
                DataContext = new InstallingViewModel(headImage, this, _mainWindowModel.GetMascotImage(),
                    _mainWindowModel.GetRepackerName(), new InstallingModel(new TimerModel()))
            },
            new FinishedView { DataContext = new FinishedViewModel(bigImage) }
        };

        CurrentTheme = "LightStandard";
        _currentViewIndex = 0;
        CurrentView = _views[_currentViewIndex];
        IconChecked = false;
        IsBackButtonVisible = false;
        IsNextButtonVisible = true;
        IsCancelButtonVisible = true;
        ButtonNextText = Strings.Next;

        ShowMessageBoxCommand = ReactiveCommand.Create(ShowMessageBox);
        ShowChatGptWindowCommand = ReactiveCommand.Create(ShowChatGptWindow);
        OpenTgLinkCommand = ReactiveCommand.Create(() => OpenLink("https://t.me/nito_kin"));
        OpenAppPurchaseLinkCommand = ReactiveCommand.Create(() => OpenLink(link));
        NavigateToNextViewCommand = ReactiveCommand.Create(NavigateNextView);
        NavigateToPreviousViewCommand = ReactiveCommand.Create(NavigatePreviousView);
        CancelCommand = ReactiveCommand.Create(ShowCloseMessageBox);
    }

    private static DrawingImage LoadSvgFromResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        var reader = new FileSvgReader(new WpfDrawingSettings());
        var drawing = reader.Read(stream);
        return new DrawingImage(drawing);
    }

    private void SetLogo(string link)
    {
        var host = new Uri(link).Host;
        var resourceName = GetResourceName(host);

        AppPurchaseLinkLogo = LoadSvgFromResource(resourceName);
    }

    private static string GetResourceName(string host)
    {
        string[] domains =
        {
            "ea.com", "store.epicgames.com", "gog.com", "blizzard.com", "xbox.com", "playstation.com",
            "store.rockstargames.com", "store.steampowered.com", "ubisoft.com", "nintendo.com"
        };

        foreach (var domain in domains)
        {
            if (host.Contains(domain))
            {
                return $"AppInstaller.Resources.{domain}.svg";
            }
        }

        return "AppInstaller.Resources.default.svg";
    }

    public void NavigateNextView()
    {
        _currentViewIndex++;

        switch (_currentViewIndex)
        {
            case 5:
                Application.Current.Shutdown();
                return;
            case 2 when _views[_currentViewIndex]?.DataContext is ReadyViewModel readyViewModel:
            {
                var driveLetter = Path.GetPathRoot(SelectedPath);
                if (driveLetter != null)
                {
                    var drive = new DriveInfo(driveLetter);
                    var availableSpace = drive.AvailableFreeSpace;

                    var memoryString = _mainWindowModel.GetNeededMemory();
                    var cleanedMemoryString =
                        new string(memoryString.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray());

                    cleanedMemoryString = cleanedMemoryString.Replace(',', '.');

                    var requiredSpaceInGb = double.Parse(cleanedMemoryString, CultureInfo.InvariantCulture);
                    var requiredSpaceInBytes = (long)(requiredSpaceInGb * 1024 * 1024 * 1024);

                    if (availableSpace < requiredSpaceInBytes + 10L * 1024 * 1024 * 1024)
                    {
                        CustomMessageBox.Show(Strings.DiskSpace);
                        _currentViewIndex--;
                        return;
                    }
                }

                readyViewModel.SelectedPath = SelectedPath;
                var additionalComponentsBuilder = new StringBuilder(Resources.Strings.AdditionalTasks);
                var selectDirViewModel = _views[1]?.DataContext as SelectDirViewModel;

                if (IconChecked)
                {
                    additionalComponentsBuilder.AppendLine().Append(" - " + Resources.Strings.CreateDesktopShortcut);
                }

                foreach (var component in selectDirViewModel?.Components)
                {
                    if (component.IsChecked)
                    {
                        additionalComponentsBuilder.AppendLine().Append(" - ").Append(component.Name);
                    }
                }

                var additionalComponentsString = additionalComponentsBuilder.ToString();
                var numberOfLines = additionalComponentsString.Split('\n').Length;

                if (numberOfLines > 1)
                {
                    readyViewModel.AdditionalComponents = additionalComponentsString;
                }

                break;
            }
            case 3 when _views[_currentViewIndex]?.DataContext is InstallingViewModel installingViewModel:
            {
                var selectDirViewModel = _views[1]?.DataContext as SelectDirViewModel;
                installingViewModel.InstallApp(AppDomain.CurrentDomain.BaseDirectory,
                    SelectedPath, _appName, _mainWindowModel.GetAppVersion(), IconChecked,
                    selectDirViewModel.Components, _mainWindowModel.GetExePaths());
                break;
            }
        }

        CurrentView = _views[_currentViewIndex];
        SetButtonProperties();
    }

    private void NavigatePreviousView()
    {
        if (_currentViewIndex <= 0) return;

        _currentViewIndex--;
        CurrentView = _views[_currentViewIndex];
        SetButtonProperties();
    }

    private void SetButtonProperties()
    {
        IsBackButtonVisible = _currentViewIndex is > 0 and < 3;
        IsNextButtonVisible = _currentViewIndex != 3;
        IsCancelButtonVisible = _currentViewIndex != 4;

        ButtonNextText = _currentViewIndex switch
        {
            2 => Resources.Strings.Install,
            4 => Resources.Strings.Finish,
            _ => Resources.Strings.Next
        };
    }

    private static void ShowChatGptWindow()
    {
        try
        {
            new ChatGptWindow().ShowDialog();
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred in MainWindowViewModel.ShowChatGptWindow(): {ex.Message}";

            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowCloseMessageBox()
    {
        var result = new CloseMessageBox().ShowDialog();
        if (result != true) return;
        ExitRequested?.Invoke();
        Application.Current.Shutdown();
    }

    private void ShowMessageBox()
    {
        try
        {
            var result = ParseXamlFormattedText(_mainWindowModel.GetRepackDescription());
            CustomMessageBox.Show(result);
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"An error occurred in MainWindowViewModel.ShowMessageBox(): {ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            CustomMessageBox.Show(errorMessage);
        }
    }

    private static FlowDocument ParseXamlFormattedText(string formattedText)
    {
        var result = formattedText.Replace("\n", "<LineBreak />").Replace("\r\n", "<LineBreak />")
            .Replace("<b>", "<Bold>").Replace("</b>", "</Bold>")
            .Replace("<u>", "<Underline>").Replace("</u>", "</Underline>")
            .Replace("<i>", "<Italic>").Replace("</i>", "</Italic>");
        result =
            $"<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
            $"<Paragraph FontSize=\"12\">{result}</Paragraph></FlowDocument>";

        return (FlowDocument)XamlReader.Parse(result);
    }

    private static void OpenLink(string? url)
    {
        if (url != null) Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private static void OnErrorMessageOccurred(string message, string caption, MessageBoxButton buttons,
        MessageBoxImage icon)
    {
        MessageBox.Show(message, caption, buttons, icon);
    }
}