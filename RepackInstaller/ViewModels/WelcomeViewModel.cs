using System.Windows.Media;
using ReactiveUI;

namespace RepackInstaller.ViewModels;

public class WelcomeViewModel : ViewModelBase
{
    // Описание игры.
    private string? _gameDescription;

    // Свойство для доступа к описанию игры.
    public string? GameDescription
    {
        get => _gameDescription;
        set => this.RaiseAndSetIfChanged(ref _gameDescription, value);
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
    
    public WelcomeViewModel(string? gameName, ImageSource? bigImage, string gameDescription)
    {
        WelcomeMessage = $"Приветствуем вас в установщике \"{gameName}\"";
        GameDescription = gameDescription;
        BigImage = bigImage;
    }
}