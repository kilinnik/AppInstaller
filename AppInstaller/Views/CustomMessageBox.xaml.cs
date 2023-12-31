﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace AppInstaller;

public partial class CustomMessageBox
{
    public CustomMessageBox()
    {
        InitializeComponent();
    }

    public static void Show(object content)
    {
        var msgBox = new CustomMessageBox();

        switch (content)
        {
            case string text:
            {
                var paragraph = new Paragraph(new Run(text))
                {
                    FontSize = 12
                };
                msgBox.MessageContent.Document = new FlowDocument(paragraph);
                break;
            }
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