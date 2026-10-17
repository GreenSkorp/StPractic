using System;
using System.Linq;
using System.Windows;

namespace StPractic.Pages.LoginElements
{
    public partial class CaptchaWindow : Window
    {
        public bool IsSuccess { get; private set; } = false;

        public CaptchaWindow()
        {
            InitializeComponent();
            GenerateCaptcha();
        }

        private void GenerateCaptcha()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var rnd = new Random();
            CaptchaTextBlock.Text = new string(
                Enumerable.Repeat(chars, 5).Select(s => s[rnd.Next(s.Length)]).ToArray()
            );
            CaptchaInput.Clear();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (CaptchaInput.Text == CaptchaTextBlock.Text)
            {
                IsSuccess = true;
                this.DialogResult = true;  
            }
            else
            {
                MessageBox.Show("Неверно введен код. Попробуйте еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                GenerateCaptcha();  
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}