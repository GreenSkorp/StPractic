using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace StPractic.Pages.LoginElements
{
    public partial class LoginPageControl : UserControl
    {
        private int _failedAttempts = 0;

        public LoginPageControl()
        {
            InitializeComponent();
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Если 3 и более ошибок — жестко требуем капчу
            if (_failedAttempts >= 3)
            {
                var captchaWin = new CaptchaWindow();
                if (captchaWin.ShowDialog() != true)
                {
                    MessageBox.Show("Авторизация отменена (капча не пройдена).", "Внимание");
                    return;
                }
            }

            string hashedPassword = GetHash(PasswordBox.Password);

            try
            {
                using (var db = new KIPLangEntities())
                {
                    var user = db.users.AsNoTracking()
                        .FirstOrDefault(u => u.login == LoginTextBox.Text && u.password_hash == hashedPassword);

                    if (user == null)
                    {
                        _failedAttempts++;
                        if (_failedAttempts >= 3)
                            MessageBox.Show("Неверный логин или пароль. Достигнут лимит попыток! Теперь потребуется капча.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                            MessageBox.Show($"Неверный логин или пароль! Осталось попыток: {3 - _failedAttempts}", "Ошибка");
                        return;
                    }

                    // Успех
                    _failedAttempts = 0;

                    // 1. Сохраняем пользователя в глобальную переменную MainWindow
                    MainWindow.CurrentUser = user;

                    // 2. Обновляем шапку (прячем кнопку "Войти", показываем профиль)
                    if (Application.Current.MainWindow is MainWindow mainWin)
                    {
                        mainWin.UpdateUI();
                    }

                    MessageBox.Show($"Добро пожаловать, {user.full_name}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 3. Перенаправляем в зависимости от роли
                    var navService = NavigationService.GetNavigationService(this);
                    if (navService != null)
                    {
                        if (user.role == "Admin" || user.role == "Teacher")
                        {
                            // Пока страниц профиля нет, отправляем на каталог
                            MessageBox.Show($"Вы вошли как {user.role}. (Страница профиля в разработке)");
                            navService.Navigate(new CourseCatalogPage());
                        }
                        else
                        {
                            // Обычный пользователь тоже пока идет в каталог
                            navService.Navigate(new CourseCatalogPage());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка БД: {ex.Message}");
            }
        }
    }
}