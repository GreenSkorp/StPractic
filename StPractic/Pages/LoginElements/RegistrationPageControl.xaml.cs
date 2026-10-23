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

            // 3. Капча
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
                    string login = LoginTextBox.Text.Trim();

                    if (db.users.Any(u => u.login == login))
                    {
                        MessageBox.Show("Логин уже занят!", "Ошибка");
                        return;
                    }

                    string hash = LoginPageControl.GetHash(password);

                    // INSERT без id — корректно работает с IDENTITY
                    // (EDMX часто не знает, что id автоинкрементный)
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO users (login, password_hash, full_name, email, role, registration_date) " +
                        "VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
                        login,
                        hash,
                        FioTextBox.Text.Trim(),
                        EmailTextBox.Text.Trim(),
                        "User",
                        DateTime.Today);
                }

                MessageBox.Show("Успешная регистрация! Теперь можете войти.", "Успех");

                LoginTextBox.Clear();
                FioTextBox.Clear();
                EmailTextBox.Clear();
                PasswordBoxFirst.Clear();
                PasswordBoxSecond.Clear();
            }
            catch (Exception ex)
            {
                string details = ex.Message;
                if (ex.InnerException != null)
                    details += "\n\n" + ex.InnerException.Message;
                if (ex.InnerException?.InnerException != null)
                    details += "\n\n" + ex.InnerException.InnerException.Message;

                MessageBox.Show("Ошибка сохранения:\n" + details, "Ошибка");
            }
        }
    }
}