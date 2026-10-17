using System.Windows;
using StPractic.Pages;

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
            // ВРЕМЕННАЯ ЗАГЛУШКА (пока нет страниц профилей)
            MessageBox.Show($"Вы вошли как: {CurrentUser.full_name}\nРоль: {CurrentUser.role}");
            // В будущем: if (CurrentUser.role == "Admin") MainFrame.Navigate(new AdminPage()); ...
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
    }
}