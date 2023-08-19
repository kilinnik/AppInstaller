using System.Windows.Media;
using ReactiveUI;

namespace AppInstaller.ViewModels;

public class WelcomeViewModel : ViewModelBase
{
    // Описание программы.
    private string? _appDescription;

    // Свойство для доступа к описанию программы.
    public string? AppDescription
    {
        get => _appDescription;
        set => this.RaiseAndSetIfChanged(ref _appDescription, value);
    }

    // Приветственное сообщение, отображаемое в представлении
    private string? _welcomeMessage;

    public string? WelcomeMessage
    {
        get => _welcomeMessage;
        set => this.RaiseAndSetIfChanged(ref _welcomeMessage, value);
    }

    // Изображение, отображаемое в представлении
    private ImageSource? _bigImage;

    public ImageSource? BigImage
    {
        get => _bigImage;
        set => this.RaiseAndSetIfChanged(ref _bigImage, value);
    }
    
    public WelcomeViewModel(string? appName, ImageSource? bigImage, string appDescription)
    {
        WelcomeMessage = $"Приветствуем вас в установщике \"{appName}\"";
        AppDescription = appDescription;
        BigImage = bigImage;
    }
}