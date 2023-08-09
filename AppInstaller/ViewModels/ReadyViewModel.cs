using System.Windows.Media;
using ReactiveUI;

namespace AppInstaller.ViewModels
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

        private string? _additionalComponents;

        // Свойство для доступа к флагу создания значка
        public string? AdditionalComponents
        {
            get => _additionalComponents;
            set => this.RaiseAndSetIfChanged(ref _additionalComponents, value);
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