using System;
using System.Windows.Controls;

namespace AppInstaller.Views;

public partial class WelcomeView
{
    public WelcomeView()
    {
        InitializeComponent();
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        const double epsilon = 0.1;

        gradientRectangle.Visibility = Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < epsilon
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;
    }
}