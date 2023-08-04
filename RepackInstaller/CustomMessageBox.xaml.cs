using System.Windows;

namespace RepackInstaller;

public partial class CustomMessageBox
{
    public CustomMessageBox()
    {
        InitializeComponent();
    }

    public static void Show(object content)
    {
        var msgBox = new CustomMessageBox
        {
            MessageContent =
            {
                Content = content // устанавливаем содержимое напрямую
            }
        };
        msgBox.ShowDialog();
    }
    
    private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }    
}