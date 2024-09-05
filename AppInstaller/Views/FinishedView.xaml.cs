using System.Diagnostics;
using System.Windows.Navigation;

namespace AppInstaller.Views
{
    public partial class FinishedView
    {
        public FinishedView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
