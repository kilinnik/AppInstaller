using System.Windows.Controls;
using System.Windows.Input;

namespace AppInstaller.Views;

public partial class WelcomeView
{
    public WelcomeView()
    {
        InitializeComponent();
    }
    
    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scrollViewer = sender as ScrollViewer;
    
        // Здесь вы можете установить количество строк для прокрутки, например, 2 строки
        const int linesToScroll = 30;
    
        // Прокручиваем количество строк, умноженное на размер шага прокрутки
        scrollViewer?.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta * linesToScroll / 120));
    
        // Отмечаем событие как обработанное, чтобы предотвратить стандартное поведение
        e.Handled = true;
    }
}