using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Windows.Media;
using AppInstaller.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using ReactiveUI;

namespace AppInstaller.ViewModels;

public class SelectDirViewModel : ViewModelBase
{
    // Ссылка на главную ViewModel
    private MainWindowViewModel MainWindowViewModel { get; }

    private string? _appName;

    public string? AppName
    {
        get => _appName;
        set => this.RaiseAndSetIfChanged(ref _appName, value);
    }

    public ObservableCollection<Components> Components { get; } = [];

    private bool _iconChecked;

    // Свойство для доступа к флагу выбора значка
    public bool IconChecked
    {
        get => _iconChecked;
        set
        {
            this.RaiseAndSetIfChanged(ref _iconChecked, value);
            // Устанавливаем значение флага в главной ViewModel
            MainWindowViewModel.IconChecked = IconChecked;
        }
    }

    private string? _selectedPath;

    // Свойство для доступа к выбранной директории установки
    public string? SelectedPath
    {
        get => _selectedPath;
        set => this.RaiseAndSetIfChanged(ref _selectedPath, value);
    }

    private string? _neededMemory;

    // Свойство для доступа к требуемой памяти
    public string? NeededMemory
    {
        get => _neededMemory;
        set => this.RaiseAndSetIfChanged(ref _neededMemory, value);
    }

    private ImageSource? _headImage;

    // Свойство для доступа к картинке игры
    public ImageSource? HeadImage
    {
        get => _headImage;
        set => this.RaiseAndSetIfChanged(ref _headImage, value);
    }
    
    private ImageSource? _bigImage;

    public ImageSource? BigImage
    {
        get => _bigImage;
        set => this.RaiseAndSetIfChanged(ref _bigImage, value);
    }

    // Команда для выбора директории установки
    public ReactiveCommand<Unit, Unit> ChooseDirectoryCommand { get; }

    public SelectDirViewModel(
        MainWindowViewModel mainWindowViewModel,
        ImageSource? headImage,
        ImageSource? bigImage,
        string? appName,
        string neededMemory,
        IEnumerable<KeyValuePair<string, string>> components
    )
    {
        MainWindowViewModel = mainWindowViewModel;
        HeadImage = headImage;
        BigImage = bigImage;
        _appName = appName;
        SelectedPath = $@"C:\Program Files (x86)\{_appName}";
        NeededMemory = neededMemory;
        foreach (var (folderName, value) in components)
        {
            var filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                folderName + ".7z.001"
            );

            if (File.Exists(filePath))
            {
                Components.Add(
                    new Components
                    {
                        Name = value,
                        FolderName = folderName,
                        IsChecked = false
                    }
                );
            }
        }

        // Создаем команду для выбора директории установки и связываем ее с методом ChooseDirectory
        ChooseDirectoryCommand = ReactiveCommand.Create(ChooseDirectory);
    }

    // Метод, который вызывается при выборе директории установки
    private void ChooseDirectory()
    {
        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = true,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
        {
            return;
        }

        SelectedPath = $@"{dialog.FileName}\{_appName}";
        MainWindowViewModel.SelectedPath = SelectedPath;
    }
}
