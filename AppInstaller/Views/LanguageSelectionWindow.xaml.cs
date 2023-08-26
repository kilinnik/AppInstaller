using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppInstaller.Views;

public partial class LanguageSelectionWindow
{
    public string? SelectedCulture { get; private set; }

    public LanguageSelectionWindow()
    {
        InitializeComponent();
    }

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            SelectedCulture = selectedItem.Tag.ToString();
        }

        DialogResult = true;
        Close();
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
    
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}