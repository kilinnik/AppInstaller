using System.Windows.Media;
using ReactiveUI;

namespace RepackInstaller.ViewModels
{
    public class ReadyViewModel : ViewModelBase
    {
        private string? _selectedPath;

        // Свойство для доступа к выбранной директории установки
        public string? SelectedPath
        {
            get => _selectedPath;
            set => this.RaiseAndSetIfChanged(ref _selectedPath, value);
        }

        private string? _createIcon;

        // Свойство для доступа к флагу создания значка
        public string? CreateIcon
        {
            get => _createIcon;
            set => this.RaiseAndSetIfChanged(ref _createIcon, value);
        }

        private ImageSource? _headImage;

        // Свойство для доступа к картинке игры
        public ImageSource? HeadImage
        {
            get => _headImage;
            set => this.RaiseAndSetIfChanged(ref _headImage, value);
        }

        public ReadyViewModel(ImageSource? headImage)
        {
            HeadImage = headImage;
        }
    }
}