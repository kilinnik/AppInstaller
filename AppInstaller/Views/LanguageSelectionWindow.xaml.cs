using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppInstaller.Views
{
    public partial class LanguageSelectionWindow
    {
        public string? SelectedCulture { get; private set; }

        public LanguageSelectionWindow()
        {
            InitializeComponent();
            SetLanguageBasedOnSystem();
        }

        private void SetLanguageBasedOnSystem()
        {
            // Получаем текущую культуру системы
            var currentCulture = CultureInfo.CurrentUICulture.Name;

            // Проверяем язык и устанавливаем текст на основе языка системы
            if (currentCulture.StartsWith("ru", StringComparison.OrdinalIgnoreCase))
            {
                LanguageComboBox.SelectedIndex = 0; // Русский
                LanguageTextBlock.Text = "Выберите язык установки";
                OkButtonTextBlock.Text = "OK";
                CancelButtonTextBlock.Text = "Выйти";
            }
            else if (currentCulture.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            {
                LanguageComboBox.SelectedIndex = 1; // English
                LanguageTextBlock.Text = "Select installation language";
                OkButtonTextBlock.Text = "OK";
                CancelButtonTextBlock.Text = "Exit";
            }
            else
            {
                // По умолчанию русский, если язык не русский и не английский
                LanguageComboBox.SelectedIndex = 0; // Русский
                LanguageTextBlock.Text = "Выберите язык установки";
                OkButtonTextBlock.Text = "OK";
                CancelButtonTextBlock.Text = "Выйти";
            }
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
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
