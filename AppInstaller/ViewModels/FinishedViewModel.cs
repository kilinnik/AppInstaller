using System.Windows.Media;
using ReactiveUI;

namespace AppInstaller.ViewModels;

public class FinishedViewModel : ViewModelBase
{
    private ImageSource? _bigImage;

    // Изображение, отображаемое в представлении
    public ImageSource? BigImage
    {
        get => _bigImage;
        set => this.RaiseAndSetIfChanged(ref _bigImage, value);
    }

    private string _repackerName;

    public string RepackerName
    {
        get => _repackerName;
        set => this.RaiseAndSetIfChanged(ref _repackerName, value);
    }
    
    private string _elapsedTime;

    public string ElapsedTime
    {
        get => _elapsedTime;
        set => this.RaiseAndSetIfChanged(ref _elapsedTime, value);
    }
    
    private string _link;

    public string Link
    {
        get => _link;
        set => this.RaiseAndSetIfChanged(ref _link, value);
    }

    public FinishedViewModel(ImageSource? bigImage, string repackerName, string link)
    {
        BigImage = bigImage;
        RepackerName = repackerName;
        Link = link;
    }
}
