using System;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Mirror.ChatGpt;
using Mirror.ChatGpt.Models.ChatGpt;
using MaterialDesignThemes.Wpf;

namespace AppInstaller.Views;

public partial class ChatGptWindow : Window
{
    private readonly ChatGptClient _chatGptClient;
    private readonly MessageEntry[] _context;
    private const int MaxRetryAttempts = 3;
    private const int DelayBetweenRetriesInSeconds = 2;
    
    public ChatGptWindow()
    {
        InitializeComponent();
        _context = new MessageEntry[50];
        _chatGptClient = InitializeChatGptClient();
    }
    
    private static ChatGptClient InitializeChatGptClient()
    {
        var services = new ServiceCollection();
        services.AddChatGptClient(new ChatGptClientOptions { ApiKey = "ApiKey" });
        var app = services.BuildServiceProvider();
        return app.GetRequiredService<ChatGptClient>();
    }
    
    private static bool IsInternetAvailable()
    {
        try
        {
            using var ping = new Ping();
            var reply = ping.Send("8.8.8.8", 2000); 
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
    
    private async void SendMessage_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MessageTextBox.Text)) return;
        
        if (!IsInternetAvailable())
        {
            MessageBox.Show(AppInstaller.Resources.Strings.NoInternetConnection);
            return;
        }
        AddMessageToGrid(MessageTextBox.Text, 150, 145);
        
        var index = Array.IndexOf(_context, null);
        if (index != -1)
        {
            _context[index] = new MessageEntry { Role = "user", Content = MessageTextBox.Text };
        }

        MessageTextBox.Text = string.Empty;
        
        var messageEntries = _context.Where(entry => entry != null).ToArray();

        var response = await RequestChatGpt(messageEntries);

        AddMessageToGrid(response, 7, 2);
    }
    
    private void AddMessageToGrid(string message, double marginTextBlock, double marginBackground)
    {
        var messageTextBox = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Text = message,
            Padding = new Thickness(3),
            Foreground = (Brush)Application.Current.Resources["TextBrush"],
            Margin = new Thickness(marginTextBlock, 2, 2, 2),
            Width = 140,
            TextWrapping = TextWrapping.Wrap,
            IsReadOnly = true,
            BorderThickness = new Thickness(0), 
            Background = Brushes.Transparent,
        };

        TextFieldAssist.SetUnderlineBrush(messageTextBox, Brushes.Transparent);

        var messageBackground = new Rectangle
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = 145,
            Height = double.NaN,
            RadiusX = 10,
            RadiusY = 10,
            Margin = new Thickness(marginBackground, 2, 2, 2),
            Fill = (Brush)Application.Current.Resources["LeftToRightGradient"]
        };

        var gridRow = new RowDefinition { Height = GridLength.Auto };
        MessagesGrid.RowDefinitions.Add(gridRow);

        var container = new Grid();
        container.Children.Add(messageBackground);
        container.Children.Add(messageTextBox);

        var translateTransform = new TranslateTransform { Y = 30 };
        container.RenderTransform = translateTransform;

        var animation = new DoubleAnimation
        {
            From = 30,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300))
        };

        animation.Completed += (s, e) => MessagesScrollViewer.ScrollToEnd();
        translateTransform.BeginAnimation(TranslateTransform.YProperty, animation);
        
        Grid.SetRow(container, MessagesGrid.RowDefinitions.Count - 1);
        MessagesGrid.Children.Add(container);
        
    }
    
    private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (sender is not TextBox textBox) return;
            var caretIndex = textBox.CaretIndex;
            textBox.Text = textBox.Text.Insert(caretIndex, Environment.NewLine);
            textBox.CaretIndex = caretIndex + Environment.NewLine.Length;
        }
        else
        {
            SendMessage_Click(sender, e);
            e.Handled = true; 
        }
    }
    
    private async Task<string> RequestChatGpt(MessageEntry[] messages)
    {
        var retryCount = 0;

        while (true)
        {
            try
            {
                var res = await _chatGptClient.ChatAsync(new ChatCompletionRequest
                {
                    Model = "gpt-3.5-turbo",
                    Messages = messages
                }, default);

                var index = Array.IndexOf(_context, null);
                _context[index] = new MessageEntry
                    { Role = res.Choices[0].Message.Role, Content = res.Choices[0].Message.Content };

                return res.Choices[0].Message.Content;
            }
            catch (HttpRequestException)
            {
                retryCount++;
                if (retryCount < MaxRetryAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(DelayBetweenRetriesInSeconds));
                }
                else
                {
                    throw;
                }
            }
        }
    }
    
    private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = MessageTextBox.Text.Trim();

        SendIcon.Kind = !string.IsNullOrWhiteSpace(text) ? PackIconKind.Send : PackIconKind.SendLock;
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Windows[1]?.Close();
    }
}