using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StPractic.Pages.LoginElements;

namespace StPractic.Pages
{
    public partial class ChangePassPage : Page
    {
        private users _currentUser;

        public ChangePassPage()
        {
            InitializeComponent();
            _currentUser = MainWindow.CurrentUser;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Сначала войдите в систему.", "Внимание");
                return;
            }

            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Заполните все поля!", "Внимание");
                return;
            }

            if (_currentUser.password_hash != LoginPageControl.GetHash(CurrentPasswordBox.Password))
            {
                MessageBox.Show("Неверный текущий пароль!", "Ошибка");
                return;
            }

            string newPass = NewPasswordBox.Password;

            string passError = RegistrationPageControl.ValidatePassword(newPass);
            if (passError != null)
            {
                MessageBox.Show(passError, "Ошибка");
                return;
            }

            if (newPass != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка");
                return;
            }

            try
            {
                using (var db = new KIPLangEntities())
                {
                    var userInDb = db.users.FirstOrDefault(u => u.id == _currentUser.id);
                    if (userInDb != null)
                    {
                        userInDb.password_hash = LoginPageControl.GetHash(newPass);
                        db.SaveChanges();

                        _currentUser.password_hash = userInDb.password_hash;
                        MainWindow.CurrentUser = _currentUser;

                        MessageBox.Show("Пароль успешно изменён!", "Успех");
                    }
                }
            }
            catch (Exception ex)
            {
                string details = ex.Message;
                if (ex.InnerException != null)
                    details += "\n\n" + ex.InnerException.Message;
                MessageBox.Show("Ошибка: " + details);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}