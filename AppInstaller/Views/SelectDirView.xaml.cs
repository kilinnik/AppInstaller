using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace AppInstaller.Views
{
    public partial class SelectDirView
    {
        public SelectDirView()
        {
            InitializeComponent();
        }

        private void CreateDesktopShortcutCheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            // Подписка на изменение состояния IsChecked
            CreateDesktopShortcutCheckBox.Checked += CreateDesktopShortcutCheckBox_Checked;
            CreateDesktopShortcutCheckBox.Unchecked += CreateDesktopShortcutCheckBox_Unchecked;

            // Устанавливаем начальное состояние (Unchecked)
            ApplyThemeColorToCheckBoxSvg(false);
        }

        private void CreateDesktopShortcutCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Применяем SVG для состояния Checked
            ApplyThemeColorToCheckBoxSvg(true);
        }

        private void CreateDesktopShortcutCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Применяем SVG для состояния Unchecked
            ApplyThemeColorToCheckBoxSvg(false);
        }

        public void ApplyThemeColorToCheckBoxSvg(bool isChecked)
        {
            // Находим CheckBox и его шаблон
            var checkBox = CreateDesktopShortcutCheckBox;

            var template = checkBox.Template;
            var checkImage = (Image)template.FindName("CheckImage", checkBox);
            if (checkImage == null || Application.Current.Resources["TextBrush"] is not SolidColorBrush textBrush)
                return;

            // Загружаем соответствующее SVG изображение в зависимости от состояния CheckBox
            var svgResourceName = isChecked 
                ? "AppInstaller.Resources.Check.svg"  // Для состояния Checked
                : "AppInstaller.Resources.NotCheck.svg"; // Для состояния Unchecked

            var svgDrawing = LoadSvgFromResource(svgResourceName, textBrush.Color);
            checkImage.Source = new DrawingImage(svgDrawing); // Используем DrawingImage для Image.Source
        }
        
        public void ApplyThemeColorToCheckBoxSvg()
        {
            // Находим CheckBox и его шаблон
            var checkBox = CreateDesktopShortcutCheckBox;

            var template = checkBox.Template;
            var checkImage = (Image)template.FindName("CheckImage", checkBox);
            if (checkImage == null || Application.Current.Resources["TextBrush"] is not SolidColorBrush textBrush)
                return;

            // Загружаем соответствующее SVG изображение
            var svgResourceName = checkBox.IsChecked == true 
                ? "AppInstaller.Resources.Check.svg"
                : "AppInstaller.Resources.NotCheck.svg";

            var svgDrawing = LoadSvgFromResource(svgResourceName, textBrush.Color);
            checkImage.Source = new DrawingImage(svgDrawing); // Обновляем изображение
        }


        private static Drawing LoadSvgFromResource(string resourceName, Color color)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);

            var reader = new FileSvgReader(new WpfDrawingSettings());
            var drawing = reader.Read(stream);

            ApplyColorToDrawing(drawing, color);
            return drawing;
        }

        private static void ApplyColorToDrawing(Drawing drawing, Color color)
        {
            switch (drawing)
            {
                case DrawingGroup drawingGroup:
                    foreach (var child in drawingGroup.Children)
                    {
                        ApplyColorToDrawing(child, color);
                    }
                    break;
                case GeometryDrawing geometryDrawing:
                    geometryDrawing.Brush = new SolidColorBrush(color);
                    break;
            }
        }
    }
}
