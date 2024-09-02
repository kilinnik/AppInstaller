using System.Diagnostics;
using System.Windows;

namespace AppInstaller.Views
{
    public partial class RuntimeWarningWindow : Window
    {
        public RuntimeWarningWindow()
        {
            InitializeComponent();
        }

        private void DownloadLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = "https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime",
                    UseShellExecute = true
                }
            );
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
