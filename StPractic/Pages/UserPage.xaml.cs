using System;
using System.Windows;
using System.Windows.Controls;

namespace StPractic.Pages
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();

            if (MainWindow.CurrentUser == null)
            {
                MessageBox.Show("Сначала войдите в систему.", "Внимание");
                return;
            }

            LoadProfileHeader();
            SetupTabsByRole();
        }

        private void LoadProfileHeader()
        {
            var user = MainWindow.CurrentUser;
            FullNameText.Text = user.full_name ?? "—";
            LoginText.Text = "@" + (user.login ?? "—");
            RoleText.Text = GetRoleDisplayName(user.role);
            RegDateText.Text = "Дата регистрации: " + user.registration_date.ToString("dd.MM.yyyy");
        }

        private static string GetRoleDisplayName(string role)
        {
            if (string.IsNullOrEmpty(role)) return "Студент";
            switch (role.ToLower())
            {
                case "admin":
                case "администратор": return "Администратор";
                case "teacher":
                case "преподаватель": return "Преподаватель";
                case "user":
                case "студент": return "Студент";
                default: return role;
            }
        }

        private void SetupTabsByRole()
        {
            // Скрыть все
            TabMyCourses.Visibility = Visibility.Collapsed;
            TabProgress.Visibility = Visibility.Collapsed;
            TabAchievements.Visibility = Visibility.Collapsed;
            TabMyReviews.Visibility = Visibility.Collapsed;
            TabTeacherCourses.Visibility = Visibility.Collapsed;
            TabStudents.Visibility = Visibility.Collapsed;
            TabTeacherStats.Visibility = Visibility.Collapsed;
            TabUsers.Visibility = Visibility.Collapsed;
            TabAllCourses.Visibility = Visibility.Collapsed;
            TabModeration.Visibility = Visibility.Collapsed;
            TabAdminStats.Visibility = Visibility.Collapsed;

            string role = (MainWindow.CurrentUser.role ?? "").ToLower();

            if (role == "admin" || role == "администратор")
            {
                TabUsers.Visibility = Visibility.Visible;
                TabAllCourses.Visibility = Visibility.Visible;
                TabModeration.Visibility = Visibility.Visible;
                TabAdminStats.Visibility = Visibility.Visible;
                MainTabs.SelectedItem = TabUsers;
            }
            else if (role == "teacher" || role == "преподаватель")
            {
                TabTeacherCourses.Visibility = Visibility.Visible;
                TabStudents.Visibility = Visibility.Visible;
                TabTeacherStats.Visibility = Visibility.Visible;
                MainTabs.SelectedItem = TabTeacherCourses;
            }
            else
            {
                TabMyCourses.Visibility = Visibility.Visible;
                TabProgress.Visibility = Visibility.Visible;
                TabAchievements.Visibility = Visibility.Visible;
                TabMyReviews.Visibility = Visibility.Visible;
                MainTabs.SelectedItem = TabMyCourses;
            }
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (!(MainTabs.SelectedItem is TabItem tab)) return;

            if (tab == TabMyCourses) CtrlMyCourses.Refresh();
            else if (tab == TabProgress) CtrlProgress.Refresh();
            else if (tab == TabAchievements) CtrlAchievements.Refresh();
            else if (tab == TabMyReviews) CtrlMyReviews.Refresh();
            else if (tab == TabTeacherCourses) CtrlTeacherCourses.Refresh();
            else if (tab == TabStudents) CtrlStudents.Refresh();
            else if (tab == TabTeacherStats) CtrlTeacherStats.Refresh();
            else if (tab == TabUsers) CtrlUsers.Refresh();
            else if (tab == TabAllCourses) CtrlAllCourses.Refresh();
            else if (tab == TabModeration) CtrlModeration.Refresh();
            else if (tab == TabAdminStats) CtrlAdminStats.Refresh();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }
    }
}