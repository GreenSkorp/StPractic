using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace StPractic.Pages.LoginElements
{
    public partial class RegistrationPageControl : UserControl
    {
        // email вида ****@***.**

    /*
         ^                    — начало строки
        [A-Za-z0-9._%+\-]+   — имя: буквы, цифры, . _ % + -
        @                    — обязательно @
        [A-Za-z0-9.\-]+      — домен: mail, gmail.com и т.п.
        \.                   — точка перед зоной
        [A-Za-z]{2,}         — зона: ru, com, org… (от 2 букв)
        $                    — конец строки
    */

        private static readonly Regex EmailRegex = new Regex(
            @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$",
            RegexOptions.Compiled);

        public RegistrationPageControl()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = (LoginTextBox.Text ?? "").Trim();
            string fio = (FioTextBox.Text ?? "").Trim();
            string email = (EmailTextBox.Text ?? "").Trim();
            string password = PasswordBoxFirst.Password ?? "";
            string password2 = PasswordBoxSecond.Password ?? "";

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(fio) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все поля!", "Внимание");
                return;
            }

            if (login.Length < 3)
            {
                MessageBox.Show("Логин должен быть не короче 3 символов.", "Внимание");
                return;
            }
            if (!login.All(c => (char.IsLetterOrDigit(c) || c == '_' || c == '-') && c < 128))
            {
                MessageBox.Show("Логин: только английские буквы, цифры, _ и -.", "Внимание");
                return;
            }

            if (fio.Length < 3)
            {
                MessageBox.Show("Введите корректное ФИО (не короче 3 символов).", "Внимание");
                return;
            }

            // ****@***.**
            if (!EmailRegex.IsMatch(email))
            {
                MessageBox.Show(
                    "Некорректный email.\n\nПример: name@example.com\n" +
                    "Нужен вид: текст@домен.зона (например user@mail.ru).",
                    "Внимание");
                return;
            }

            string passError = ValidatePassword(password);
            if (passError != null)
            {
                MessageBox.Show(passError, "Внимание");
                return;
            }

            if (password != password2)
            {
                MessageBox.Show("Пароли не совпадают!", "Внимание");
                return;
            }

            var captchaWin = new CaptchaWindow();
            if (captchaWin.ShowDialog() != true)
            {
                MessageBox.Show("Регистрация отменена: капча не пройдена.", "Внимание");
                return;
            }

            try
            {
                using (var db = new KIPLangEntities())
                {
                    if (db.users.Any(u => u.login == login))
                    {
                        MessageBox.Show("Логин уже занят!", "Ошибка");
                        return;
                    }

                    if (db.users.Any(u => u.email == email))
                    {
                        MessageBox.Show("Этот email уже зарегистрирован!", "Ошибка");
                        return;
                    }

                    string hash = LoginPageControl.GetHash(password);

                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO users (login, password_hash, full_name, email, role, registration_date) " +
                        "VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
                        login,
                        hash,
                        fio,
                        email,
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

        /// <summary>
        /// ≥6 символов, есть буква, есть цифра, только латиница и цифры.
        /// null = ок, иначе текст ошибки.
        /// </summary>
        public static string ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return "Пароль должен быть не короче 6 символов.";

            if (!password.Any(char.IsLetter))
                return "Пароль должен содержать хотя бы одну букву (A–Z, a–z).";

            if (!password.Any(char.IsDigit))
                return "Пароль должен содержать хотя бы одну цифру (0–9).";

            if (!password.All(c => char.IsLetterOrDigit(c) && c < 128))
                return "Пароль: только английские буквы и цифры (без пробелов и спецсимволов).";

            return null;
        }
    }
}