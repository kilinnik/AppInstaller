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
    public event Action<string>? ThemeChanged;

    private readonly MainWindowModel _mainWindowModel;

    private readonly UserControl?[] _views;

    private readonly string _appName;

    private readonly IEnumerable<KeyValuePair<string, string>> _components;

    private double _appPurchaseLinkImageWidth;

    public double AppPurchaseLinkImageWidth
    {
        get => _appPurchaseLinkImageWidth;
        set => this.RaiseAndSetIfChanged(ref _appPurchaseLinkImageWidth, value);
    }

    private double _appPurchaseLinkImageHeight;

    public double AppPurchaseLinkImageHeight
    {
        get => _appPurchaseLinkImageHeight;
        set => this.RaiseAndSetIfChanged(ref _appPurchaseLinkImageHeight, value);
    }

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
        get
        {
            if (_views[1] is SelectDirView selectDirView)
            {
                selectDirView.ApplyThemeColorToCheckBoxSvg();
            }

            return _appPurchaseLinkLogo;
        }
        set => this.RaiseAndSetIfChanged(ref _appPurchaseLinkLogo, value);
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

            UpdateThemeIcon(value);
        }
    }

    private string _themeIconSource;

    public string ThemeIconSource
    {
        get => _themeIconSource;
        set => this.RaiseAndSetIfChanged(ref _themeIconSource, value);
    }

    private double _themeIconWidth;

    public double ThemeIconWidth
    {
        get => _themeIconWidth;
        set => this.RaiseAndSetIfChanged(ref _themeIconWidth, value);
    }

    private double _themeIconHeight;

    public double ThemeIconHeight
    {
        get => _themeIconHeight;
        set => this.RaiseAndSetIfChanged(ref _themeIconHeight, value);
    }

    private int _currentViewIndex;

    public int CurrentViewIndex
    {
        get => _currentViewIndex;
        set => this.RaiseAndSetIfChanged(ref _currentViewIndex, value);
    }

    private bool _isDefaultLogo;

    public bool IsDefaultLogo
    {
        get => _isDefaultLogo;
        set => this.RaiseAndSetIfChanged(ref _isDefaultLogo, value);
    }

    public ReactiveCommand<Unit, Unit> ShowMessageBoxCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenTgLinkCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenAppPurchaseLinkCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToNextViewCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToPreviousViewCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public event Action ExitRequested;

    public MainWindowViewModel(string initialTheme)
    {
        _mainWindowModel = new MainWindowModel();
        _mainWindowModel.ErrorMessageOccurred += OnErrorMessageOccurred;
        _components = _mainWindowModel.GetComponentNames();
        _appName = _mainWindowModel.GetAppName();
        AppTheme = _mainWindowModel.GetAppTheme();
        var bigImage = _mainWindowModel.GetBigImage();
        var headImage = _mainWindowModel.GetHeadImage();
        var link = _mainWindowModel.GetAppPurchaseLink();
        SetLogo(link);

        _views =
        [
            new WelcomeView
            {
                DataContext = new WelcomeViewModel(
                    _appName,
                    bigImage,
                    _mainWindowModel.GetAppDescription()
                )
            },
            new SelectDirView
            {
                DataContext = new SelectDirViewModel(
                    this,
                    headImage,
                    bigImage,
                    _appName,
                    _mainWindowModel.GetNeededMemory(),
                    _components
                )
            },
            new ReadyView { DataContext = new ReadyViewModel(headImage) },
            new InstallingView
            {
                DataContext = new InstallingViewModel(
                    headImage,
                    this,
                    _mainWindowModel.GetMascotImage(),
                    _mainWindowModel.GetRepackerName(),
                    new InstallingModel(new TimerModel()),
                    AppTheme
                )
            },
            new FinishedView
            {
                DataContext = new FinishedViewModel(bigImage, _mainWindowModel.GetRepackerName(), AppTheme)
            }
        ];

        CurrentTheme = initialTheme;
        _currentViewIndex = 0;
        CurrentView = _views[_currentViewIndex];
        IconChecked = false;
        IsBackButtonVisible = false;
        IsNextButtonVisible = true;
        IsCancelButtonVisible = false;

        SetButtonProperties();

        ShowMessageBoxCommand = ReactiveCommand.Create(ShowMessageBox);
        OpenTgLinkCommand = ReactiveCommand.Create(
            () => OpenLink("https://t.me/+Ds3HUgrpVWU3MGNi")
        );
        OpenAppPurchaseLinkCommand = ReactiveCommand.Create(() => OpenLink(link));
        NavigateToNextViewCommand = ReactiveCommand.Create(NavigateNextView);
        NavigateToPreviousViewCommand = ReactiveCommand.Create(NavigatePreviousView);
        CancelCommand = ReactiveCommand.Create(ShowCloseMessageBox);
    }

    private void UpdateThemeIcon(string theme)
    {
        if (theme.Contains("Dark"))
        {
            ThemeIconSource = "/Resources/light.svg";
            ThemeIconWidth = 17;
            ThemeIconHeight = 17;
        }
        else
        {
            ThemeIconSource = "/Resources/dark.svg";
            ThemeIconWidth = 15;
            ThemeIconHeight = 15;
        }
    }

    private static DrawingImage LoadSvgFromResource(string resourceName, bool isDefault = false)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        var reader = new FileSvgReader(new WpfDrawingSettings());
        var drawing = reader.Read(stream);

        if (!isDefault) return new DrawingImage(drawing);

        // Получаем динамический ресурс для цвета и применяем его
        if (Application.Current.Resources["IconBrush"] is SolidColorBrush iconBrush)
        {
            ApplyColorToDrawing(drawing, iconBrush.Color);
        }

        return new DrawingImage(drawing);
    }

    private static void ApplyColorToDrawing(Drawing drawing, Color color)
    {
        switch (drawing)
        {
            case DrawingGroup drawingGroup:
            {
                foreach (var child in drawingGroup.Children)
                {
                    ApplyColorToDrawing(child, color);
                }

                break;
            }
            case GeometryDrawing geometryDrawing:
                geometryDrawing.Brush = new SolidColorBrush(color);
                break;
        }
    }

    private void SetLogo(string link)
    {
        var host = new Uri(link).Host;
        var resourceName = GetResourceName(host);

        var isDefault = resourceName == "AppInstaller.Resources.default.svg";
        AppPurchaseLinkLogo = LoadSvgFromResource(resourceName, isDefault);

        // Устанавливаем флаг, что лого дефолтное
        IsDefaultLogo = isDefault;

        // Устанавливаем размеры изображения в зависимости от того, дефолтное ли это лого
        if (IsDefaultLogo)
        {
            // Размеры для дефолтного логотипа
            AppPurchaseLinkImageWidth = 22;
            AppPurchaseLinkImageHeight = 22;
        }
        else
        {
            // Размеры для кастомного логотипа
            AppPurchaseLinkImageWidth = 24;
            AppPurchaseLinkImageHeight = 24;
        }
    }

    private static string GetResourceName(string host)
    {
        string[] domains =
        [
            "store.epicgames.com",
            "gog.com",
            "xbox.com",
            "playstation.com",
            "store.steampowered.com",
            "nintendo.com"
        ];

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
        CurrentViewIndex++;

        switch (_currentViewIndex)
        {
            case 5:
                Process.Start(
                    new ProcessStartInfo("https://nitokinoff.org/") { UseShellExecute = true }
                );
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
                    var cleanedMemoryString = new string(
                        memoryString.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray()
                    );

                    cleanedMemoryString = cleanedMemoryString.Replace(',', '.');

                    var requiredSpaceInGb = double.Parse(
                        cleanedMemoryString,
                        CultureInfo.InvariantCulture
                    );
                    var requiredSpaceInBytes = (long)(requiredSpaceInGb * 1024 * 1024 * 1024);

                    if (availableSpace < requiredSpaceInBytes + 10L * 1024 * 1024 * 1024)
                    {
                        CustomMessageBox.Show(Strings.DiskSpace);
                        _currentViewIndex--;
                        return;
                    }
                }

                readyViewModel.SelectedPath = SelectedPath;
                var additionalComponentsBuilder = new StringBuilder();
                var selectDirViewModel = _views[1]?.DataContext as SelectDirViewModel;

                if (IconChecked)
                {
                    additionalComponentsBuilder.AppendLine(" •  " + Strings.CreateDesktopShortcut);
                }

                foreach (var component in selectDirViewModel?.Components)
                {
                    if (component.IsChecked)
                    {
                        additionalComponentsBuilder
                            .AppendLine()
                            .Append(" - ")
                            .Append(component.Name);
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
            case 3
                when _views[_currentViewIndex]?.DataContext
                    is InstallingViewModel installingViewModel:
            {
                var selectDirViewModel = _views[1]?.DataContext as SelectDirViewModel;
                installingViewModel.InstallApp(
                    AppDomain.CurrentDomain.BaseDirectory,
                    SelectedPath,
                    _appName,
                    _mainWindowModel.GetAppVersion(),
                    IconChecked,
                    selectDirViewModel.Components,
                    _mainWindowModel.GetExePaths()
                );
                break;
            }
            case 4
                when _views[_currentViewIndex]?.DataContext is FinishedViewModel finishedViewModel:
            {
                if (_views[3]?.DataContext is InstallingViewModel installingViewModel)
                {
                    var elapsedTime = installingViewModel.ElapsedTime;
                    finishedViewModel.ElapsedTime =
                        elapsedTime.Length >= 8 ? elapsedTime[^8..] : elapsedTime;
                }

                break;
            }
        }

        CurrentView = _views[_currentViewIndex];
        SetButtonProperties();
    }

    private void NavigatePreviousView()
    {
        if (_currentViewIndex <= 0)
            return;

        CurrentViewIndex--;
        CurrentView = _views[_currentViewIndex];
        SetButtonProperties();
    }

    private void SetButtonProperties()
    {
        IsBackButtonVisible = _currentViewIndex is > 0 and < 3;
        IsNextButtonVisible = _currentViewIndex != 3;
        IsCancelButtonVisible = _currentViewIndex == 3;

        ButtonNextText = _currentViewIndex switch
        {
            0 => Strings.Start,
            2 => Strings.Install,
            4 => Strings.Finish,
            _ => Strings.Next
        };
    }

    private void ShowCloseMessageBox()
    {
        // Показать затемнение
        var mainWindow = Application.Current.MainWindow as MainWindow;
        if (mainWindow != null)
        {
            mainWindow.Overlay.Visibility = Visibility.Visible;
        }

        var closeMessageBox = new CloseMessageBox
        {
            Owner = mainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner // Центрировать новое окно относительно MainWindow
        };

        var result = closeMessageBox.ShowDialog();

        // Убрать затемнение
        if (mainWindow != null)
        {
            mainWindow.Overlay.Visibility = Visibility.Collapsed;
        }

        if (result != true)
            return;

        ExitRequested?.Invoke();
        Application.Current.Shutdown();
    }

    private void ShowMessageBox()
    {
        var mainWindow = Application.Current.MainWindow as MainWindow;
        if (mainWindow != null)
        {
            mainWindow.Overlay.Visibility = Visibility.Visible;
        }

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
                errorMessage +=
                    $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            CustomMessageBox.Show(errorMessage);
        }
        finally
        {
            if (mainWindow != null)
            {
                mainWindow.Overlay.Visibility = Visibility.Collapsed;
            }
        }
    }

    private static FlowDocument ParseXamlFormattedText(string formattedText)
    {
        var result = formattedText
            .Replace("\n", "<LineBreak />")
            .Replace("\r\n", "<LineBreak />")
            .Replace("<b>", "<Bold>")
            .Replace("</b>", "</Bold>")
            .Replace("<u>", "<Underline>")
            .Replace("</u>", "</Underline>")
            .Replace("<i>", "<Italic>")
            .Replace("</i>", "</Italic>");
        result =
            $"<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">"
            + $"<Paragraph FontSize=\"14\" FontWeight=\"Regular\" FontFamily=\"/Resources/#Inter\">{result}</Paragraph></FlowDocument>";

        return (FlowDocument)XamlReader.Parse(result);
    }

    private static void OpenLink(string? url)
    {
        if (url != null)
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private static void OnErrorMessageOccurred(
        string message,
        string caption,
        MessageBoxButton buttons,
        MessageBoxImage icon
    )
    {
        MessageBox.Show(message, caption, buttons, icon);
    }
}