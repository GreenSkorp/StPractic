using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StPractic.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginPageControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void LightTheme_Checked(object sender, RoutedEventArgs e)
        {
            SwitchTheme("LightTheme.xaml");
        }

        private void DarkTheme_Checked(object sender, RoutedEventArgs e)
        {
            SwitchTheme("DarkTheme.xaml");
        }

        private void SwitchTheme(string themeFileName)
        {
            // Защита от вызова до инициализации Application
            if (Application.Current == null) return;

            try
            {
                var uri = new Uri($"pack://application:,,,/res/{themeFileName}",
                                  UriKind.Absolute);

                var dicts = Application.Current.Resources.MergedDictionaries;

                var oldTheme = dicts.FirstOrDefault(d =>
                    d.Source != null &&
                    d.Source.OriginalString.Contains("Theme"));

                if (oldTheme != null)
                    dicts.Remove(oldTheme);

                dicts.Add(new ResourceDictionary { Source = uri });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить тему '{themeFileName}':\n{ex.Message}",
                                "Ошибка темы",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }
    }
}