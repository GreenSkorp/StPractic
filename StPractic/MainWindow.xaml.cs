using StPractic.Pages;
using System;
using System.Linq;
using System.Windows;

namespace StPractic
{
    public partial class MainWindow : Window
    {
        // Статическая переменная, чтобы любая страница знала, кто вошел
        public static users CurrentUser { get; set; } = null;

        public MainWindow()
        {
            InitializeComponent();
            UpdateUI();
        }

        // Обновление кнопок в шапке в зависимости от авторизации
        public void UpdateUI()
        {
            if (CurrentUser == null)
            {
                LoginBtn.Visibility = Visibility.Visible;
                AuthPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoginBtn.Visibility = Visibility.Collapsed;
                AuthPanel.Visibility = Visibility.Visible;
            }
        }

        private void Logo_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CourseCatalogPage());
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LoginPage());
        }

        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.role == "Admin") MainFrame.Navigate(new UserPage());
                    }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                CurrentUser = null;
                UpdateUI();
                MainFrame.Navigate(new CourseCatalogPage());
            }
        }

        private void ThemeSwitch_Checked(object sender, RoutedEventArgs e)
        {
            SwitchTheme("DarkTheme.xaml");
        }

        private void ThemeSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            SwitchTheme("LightTheme.xaml");
        }

        private void SwitchTheme(string themeFileName)
        {
            if (Application.Current == null) return;
            try
            {
                var uri = new Uri($"pack://application:,,,/res/{themeFileName}", UriKind.Absolute);
                var dicts = Application.Current.Resources.MergedDictionaries;
                var oldTheme = dicts.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme"));

                if (oldTheme != null) dicts.Remove(oldTheme);
                dicts.Add(new ResourceDictionary { Source = uri });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки темы: {ex.Message}");
            }
        }
    }
}