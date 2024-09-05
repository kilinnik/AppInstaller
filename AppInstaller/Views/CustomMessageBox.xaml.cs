using System.Windows;
using System.Windows.Documents;

namespace AppInstaller.Views;

public partial class CustomMessageBox
{
    public CustomMessageBox()
    {
        InitializeComponent();
    }

    public static void Show(object content)
    {
        var mainWindow = Application.Current.MainWindow as MainWindow;
        var msgBox = new CustomMessageBox
        {
            Owner = mainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        switch (content)
        {
            case string text:
                var paragraph = new Paragraph(new Run(text))
                {
                    FontSize = 14
                };
                msgBox.MessageContent.Document = new FlowDocument(paragraph);
                break;
            case FlowDocument flowDocument:
                msgBox.MessageContent.Document = flowDocument;
                break;
        }

        msgBox.MessageContent.FontSize = 8;
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