using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using StPractic.Pages.LoginElements;

namespace StPractic.Pages
{
    public partial class ChangePassPage : Page
    {
        // Сюда нужно будет передавать текущего пользователя, когда будете делать профиль
        private users _currentUser;

        public ChangePassPage(users user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Заполните все поля!", "Внимание");
                return;
            }

            // Проверка текущего пароля
            if (_currentUser.password_hash != LoginPageControl.GetHash(CurrentPasswordBox.Password))
            {
                MessageBox.Show("Неверный текущий пароль!", "Ошибка");
                return;
            }

            // Проверка нового пароля
            string newPass = NewPasswordBox.Password;
            if (newPass.Length < 6 || !newPass.Any(char.IsDigit) || !newPass.All(c => char.IsLetterOrDigit(c) && c < 128))
            {
                MessageBox.Show("Пароль должен быть от 6 символов, содержать цифру и только английские буквы!", "Ошибка");
                return;
            }

            if (newPass != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка");
                return;
            }

            // Сохранение в БД
            try
            {
                using (var db = new KIPLangEntities())
                {
                    var userInDb = db.users.FirstOrDefault(u => u.id == _currentUser.id);
                    if (userInDb != null)
                    {
                        userInDb.password_hash = LoginPageControl.GetHash(newPass);
                        db.SaveChanges();
                        MessageBox.Show("Пароль успешно изменен!", "Успех");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}