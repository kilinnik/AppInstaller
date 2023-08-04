using System.Windows.Media;
using ReactiveUI;

namespace RepackInstaller.ViewModels;

public class FinishedViewModel: ViewModelBase
{
    private ImageSource? _bigImage;
    
    // Изображение, отображаемое в представлении
    public ImageSource? BigImage
    {
        get => _bigImage;
        set => this.RaiseAndSetIfChanged(ref _bigImage, value);
    }
    
    public FinishedViewModel(ImageSource? bigImage)
    {
        BigImage = bigImage;
    }
}