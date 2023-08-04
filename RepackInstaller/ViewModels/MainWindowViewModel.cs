using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ReactiveUI;
using RepackInstaller.Models;
using RepackInstaller.Views;

namespace RepackInstaller.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly MainWindowModel _mainWindowModel;

    private int _currentViewIndex;

    private readonly UserControl?[] _views;

    private readonly string _gameName;

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

    private string? _gameTitleDisplay;

    public string? GameTitleDisplay
    {
        get => _gameTitleDisplay;
        set => this.RaiseAndSetIfChanged(ref _gameTitleDisplay, value);
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

    public ReactiveCommand<Unit, Unit> ShowMessageBoxCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenTgLinkCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenSteamLinkCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToNextViewCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToPreviousViewCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public MainWindowViewModel()
    {
        _mainWindowModel = new MainWindowModel();
        _mainWindowModel.ErrorMessageOccurred += OnErrorMessageOccurred;
        _gameName = _mainWindowModel.GetGameName();
        GameTitleDisplay = $"R. G. NITOKIN - {_mainWindowModel.GetGameName()}";
        var bigImage = _mainWindowModel.GetBigImage();
        var headImage = _mainWindowModel.GetHeadImage();

        _views = new UserControl?[]
        {
            new WelcomeView
            {
                DataContext = new WelcomeViewModel(_gameName, bigImage, _mainWindowModel.GetGameDescription())
            },
            new SelectDirView
            {
                DataContext = new SelectDirViewModel(this, headImage, _gameName,
                    "Занимаемый файлами размер: " + _mainWindowModel.GetNeededMemory())
            },
            new ReadyView { DataContext = new ReadyViewModel(headImage) },
            new InstallingView
            {
                DataContext = new InstallingViewModel(headImage, this, _mainWindowModel.GetMascotImage(),
                    _mainWindowModel.GetRepackerName(), new InstallingModel(new TimerModel()))
            },
            new FinishedView { DataContext = new FinishedViewModel(bigImage) }
        };

        _currentViewIndex = 0;
        CurrentView = _views[_currentViewIndex];
        IconChecked = false;
        IsBackButtonVisible = false;
        IsNextButtonVisible = true;
        IsCancelButtonVisible = true;
        ButtonNextText = "< Вперед";

        ShowMessageBoxCommand = ReactiveCommand.Create(ShowMessageBox);
        OpenTgLinkCommand = ReactiveCommand.Create(() => OpenLink("https://t.me/nito_kin"));
        OpenSteamLinkCommand = ReactiveCommand.Create(() => OpenLink(_mainWindowModel.GetSteamGameLink()));
        NavigateToNextViewCommand = ReactiveCommand.Create(NavigateNextView);
        NavigateToPreviousViewCommand = ReactiveCommand.Create(NavigatePreviousView);
        CancelCommand = ReactiveCommand.Create(ShowCloseMessageBox);
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
                readyViewModel.SelectedPath = "Папка установки: " + SelectedPath;
                if (IconChecked)
                {
                    readyViewModel.CreateIcon = "Дополнительные задачи:\n - Создать иконку на рабочем столе";
                }

                break;
            }
            case 3 when _views[_currentViewIndex]?.DataContext is InstallingViewModel installingViewModel:
            {
                installingViewModel.InstallGame(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.bin"),
                    SelectedPath, $@"{SelectedPath}\{_mainWindowModel.GetGameExePath()}",
                    _mainWindowModel.GetGameName(),
                    _mainWindowModel.GetGameVersion(),
                    IconChecked);
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
            2 => "Установить",
            4 => "Завершить",
            _ => "< Вперед"
        };
    }

    private static void ShowCloseMessageBox()
    {
        var result = new CloseMessageBox().ShowDialog();
        if (result == true)
        {
            Application.Current.Shutdown();
        }
    }

    private static void ShowMessageBox()
    {
        try
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
            if (!File.Exists(filePath))
            {
                CustomMessageBox.Show(new TextBlock { Text = "File not found!" });
                return;
            }

            var content = File.ReadAllText(filePath);
            var startIndex = content.IndexOf("RepackDescription=", StringComparison.Ordinal) +
                             "RepackDescription=".Length;
            var endIndex = content.IndexOf("\nGameSteamLink=", startIndex, StringComparison.Ordinal);

            if (startIndex >= "RepackInfo=".Length && endIndex >= 0)
            {
                var result = ParseXamlFormattedText(content.Substring(startIndex, endIndex - startIndex));

                CustomMessageBox.Show(result);
            }
            else
            {
                CustomMessageBox.Show(new TextBlock { Text = "Substring not found!" });
            }
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show(new TextBlock { Text = $"An error occurred: {ex.Message}" });
        }
    }

    private static TextBlock ParseXamlFormattedText(string formattedText)
    {
        var result = formattedText.Replace("\n", "<LineBreak />").Replace("\r\n", "<LineBreak />")
            .Replace("<b>", "<Bold>").Replace("</b>", "</Bold>")
            .Replace("<u>", "<Underline>").Replace("</u>", "</Underline>")
            .Replace("<i>", "<Italic>").Replace("</i>", "</Italic>");
        result =
            "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" TextWrapping=\"Wrap\">" +
            result + "</TextBlock>";

        return (TextBlock)XamlReader.Parse(result);
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