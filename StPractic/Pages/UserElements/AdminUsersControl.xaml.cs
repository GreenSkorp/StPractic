using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages.UserElements
{
    public partial class AdminUsersControl : UserControl
    {
        public AdminUsersControl()
        {
            InitializeComponent();
            Loaded += (s, e) => Refresh();
        }

        public void Refresh()
        {
            LoadUsers();
        }

public void LoadUsers(string search = "")
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    var users = db.users.OrderBy(u => u.id).ToList();

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        string s = search.ToLower();
                        users = users.Where(u =>
                            (u.full_name != null && u.full_name.ToLower().Contains(s)) ||
                            (u.login != null && u.login.ToLower().Contains(s))).ToList();
                    }

                    UsersGrid.ItemsSource = users.Select(u => new UserItem
                    {
                        Id = u.id,
                        FullName = u.full_name,
                        Login = u.login,
                        Role = u.role,
                        DateText = u.registration_date.ToString("dd.MM.yyyy")
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message);
            }
        }

private void UserSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadUsers(UserSearchBox.Text.Trim());
        }

private void UsersGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(UsersGrid.SelectedItem is UserItem user)) return;

            var choice = MessageBox.Show(
                "Пользователь: " + user.FullName + "\nТекущая роль: " + user.Role +
                "\n\nДа — сделать Преподавателем\nНет — сделать Студентом\nОтмена — ничего не менять",
                "Смена роли",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (choice == MessageBoxResult.Cancel) return;

            string newRole = choice == MessageBoxResult.Yes ? "Teacher" : "User";

            try
            {
                using (var db = new KIPLangEntities())
                {
                    var u = db.users.FirstOrDefault(x => x.id == user.Id);
                    if (u == null) return;
                    if (u.role == "Admin" || u.role == "Администратор")
                    {
                        MessageBox.Show("Нельзя менять роль администратора.");
                        return;
                    }
                    u.role = newRole;
                    db.SaveChanges();
                }
                LoadUsers(UserSearchBox.Text.Trim());
                MessageBox.Show("Роль изменена на: " + newRole);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
