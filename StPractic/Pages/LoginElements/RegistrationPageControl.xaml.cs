using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StPractic.Pages.LoginElements
{
    public partial class RegistrationPageControl : UserControl
    {
        public RegistrationPageControl()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверка заполнения
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) || string.IsNullOrWhiteSpace(FioTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBoxFirst.Password))
            {
                MessageBox.Show("Заполните все поля!", "Внимание");
                return;
            }

            // 2. Проверка пароля
            string password = PasswordBoxFirst.Password;
            if (password.Length < 6 || !password.Any(char.IsDigit) || !password.All(c => char.IsLetterOrDigit(c) && c < 128))
            {
                MessageBox.Show("Пароль должен быть от 6 символов, содержать цифру и только английские буквы/цифры!", "Внимание");
                return;
            }

            if (password != PasswordBoxSecond.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Внимание");
                return;
            }

            // 3. Капча перед сохранением!
            var captchaWin = new CaptchaWindow();
            if (captchaWin.ShowDialog() != true)
            {
                MessageBox.Show("Регистрация отменена: капча не пройдена.", "Внимание");
                return;
            }

            // 4. Сохранение в БД
            try
            {
                using (var db = new KIPLangEntities())
                {
                    if (db.users.Any(u => u.login == LoginTextBox.Text))
                    {
                        MessageBox.Show("Логин уже занят!", "Ошибка");
                        return;
                    }

                    var newUser = new users
                    {
                        login = LoginTextBox.Text,
                        full_name = FioTextBox.Text,
                        email = EmailTextBox.Text,
                        password_hash = LoginPageControl.GetHash(password),
                        role = "User",
                        registration_date = DateTime.Now
                    };

                    db.users.Add(newUser);
                    db.SaveChanges();
                }

                MessageBox.Show("Успешная регистрация! Теперь можете войти.", "Успех");

                // Очищаем поля
                LoginTextBox.Clear(); FioTextBox.Clear(); EmailTextBox.Clear();
                PasswordBoxFirst.Clear(); PasswordBoxSecond.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка");
            }
        }
    }
}