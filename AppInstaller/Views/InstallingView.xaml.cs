using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace AppInstaller.Views;

public partial class InstallingView
{
    public InstallingView()
    {
        InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
