using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages
{
    public partial class UserPage : Page
    {
        // ===== Модели для привязки (как CourseItem в каталоге) =====
        public class CourseCardItem
        {
            public int CourseId { get; set; }
            public string Title { get; set; }
            public string LevelInfo { get; set; }
            public double Progress { get; set; }
            public string ProgressText { get; set; }
        }

        public class ProgressItem
        {
            public string CourseTitle { get; set; }
            public string LessonTitle { get; set; }
            public string Status { get; set; }
            public string Score { get; set; }
            public string DateText { get; set; }
        }

        public class AchievementItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string DateText { get; set; }
            public double Opacity { get; set; } = 1.0;
        }

        public class ReviewItem
        {
            public string CourseTitle { get; set; }
            public string Stars { get; set; }
            public string Comment { get; set; }
            public string DateText { get; set; }
        }

        public class TeacherCourseItem
        {
            public int CourseId { get; set; }
            public string Title { get; set; }
            public string StudentsText { get; set; }
            public string StatusText { get; set; }
        }

        public class StudentItem
        {
            public string FullName { get; set; }
            public string CourseTitle { get; set; }
            public string DateText { get; set; }
            public string Status { get; set; }
        }

        public class UserItem
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public string Login { get; set; }
            public string Role { get; set; }
            public string DateText { get; set; }
        }

        public class CourseAdminItem
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Teacher { get; set; }
            public string Level { get; set; }
            public string PriceText { get; set; }
        }

        public class ModerationItem
        {
            public int ReviewId { get; set; }
            public string Header { get; set; }
            public string Comment { get; set; }
            public string DateText { get; set; }
        }

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

        // ===================== ШАПКА =====================
        private void LoadProfileHeader()
        {
            var user = MainWindow.CurrentUser;
            FullNameText.Text = user.full_name ?? "—";
            LoginText.Text = "@" + (user.login ?? "—");
            RoleText.Text = GetRoleDisplayName(user.role);
            RegDateText.Text = "Дата регистрации: " + user.registration_date.ToString("dd.MM.yyyy");
        }

        private string GetRoleDisplayName(string role)
        {
            if (string.IsNullOrEmpty(role)) return "Студент";
            switch (role.Trim().ToLower())
            {
                case "admin":
                case "администратор":
                    return "Администратор";
                case "teacher":
                case "преподаватель":
                    return "Преподаватель";
                default:
                    return "Студент";
            }
        }

        private string GetRoleKey()
        {
            var role = (MainWindow.CurrentUser.role ?? "User").Trim().ToLower();
            if (role == "admin" || role == "администратор") return "Admin";
            if (role == "teacher" || role == "преподаватель") return "Teacher";
            return "User";
        }

        // ===================== ВКЛАДКИ ПО РОЛИ =====================
        private void SetupTabsByRole()
        {
            string roleKey = GetRoleKey();

            // Скрываем все вкладки
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

            if (roleKey == "Admin")
            {
                TabUsers.Visibility = Visibility.Visible;
                TabAllCourses.Visibility = Visibility.Visible;
                TabModeration.Visibility = Visibility.Visible;
                TabAdminStats.Visibility = Visibility.Visible;
                MainTabs.SelectedItem = TabUsers;
                LoadUsers();
            }
            else if (roleKey == "Teacher")
            {
                TabTeacherCourses.Visibility = Visibility.Visible;
                TabStudents.Visibility = Visibility.Visible;
                TabTeacherStats.Visibility = Visibility.Visible;
                MainTabs.SelectedItem = TabTeacherCourses;
                LoadTeacherCourses();
            }
            else
            {
                TabMyCourses.Visibility = Visibility.Visible;
                TabProgress.Visibility = Visibility.Visible;
                TabAchievements.Visibility = Visibility.Visible;
                TabMyReviews.Visibility = Visibility.Visible;
                MainTabs.SelectedItem = TabMyCourses;
                LoadMyCourses();
            }
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (MainTabs.SelectedItem is TabItem tab)
            {
                if (tab == TabMyCourses) LoadMyCourses();
                else if (tab == TabProgress) LoadProgress();
                else if (tab == TabAchievements) LoadAchievements();
                else if (tab == TabMyReviews) LoadMyReviews();
                else if (tab == TabTeacherCourses) LoadTeacherCourses();
                else if (tab == TabStudents) LoadStudents();
                else if (tab == TabUsers) LoadUsers();
                else if (tab == TabAllCourses) LoadAllCourses();
                else if (tab == TabModeration) LoadModeration();
                else if (tab == TabAdminStats) LoadAdminStats();
            }
        }

        // ===================== СТУДЕНТ =====================
        private void LoadMyCourses()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    int userId = MainWindow.CurrentUser.id;

                    var raw = (from en in db.enrollments
                               join c in db.courses on en.course_id equals c.id
                               where en.user_id == userId && en.status != "cancelled"
                               select new { c.id, c.title, c.level }).ToList();

                    var list = new List<CourseCardItem>();
                    foreach (var x in raw)
                    {
                        int total = db.lessons.Count(l => l.course_id == x.id);
                        int done = db.user_progress.Count(p =>
                            p.user_id == userId &&
                            p.status == "completed" &&
                            db.lessons.Any(l => l.id == p.lesson_id && l.course_id == x.id));

                        double progress = total > 0 ? Math.Round(100.0 * done / total) : 0;

                        list.Add(new CourseCardItem
                        {
                            CourseId = x.id,
                            Title = x.title,
                            LevelInfo = x.level ?? "",
                            Progress = progress,
                            ProgressText = total > 0 ? $"Прогресс: {progress}%" : "Прогресс: —"
                        });
                    }

                    MyCoursesList.ItemsSource = list;
                    NoCoursesText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки курсов: " + ex.Message);
            }
        }

        private void LoadProgress()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    int userId = MainWindow.CurrentUser.id;

                    var raw = (from p in db.user_progress
                               join l in db.lessons on p.lesson_id equals l.id
                               join c in db.courses on l.course_id equals c.id
                               where p.user_id == userId
                               orderby p.completed_at descending
                               select new
                               {
                                   CourseTitle = c.title,
                                   LessonTitle = l.title,
                                   p.status,
                                   p.score,
                                   p.completed_at
                               }).ToList();

                    var list = raw.Select(x => new ProgressItem
                    {
                        CourseTitle = x.CourseTitle,
                        LessonTitle = x.LessonTitle,
                        Status = x.status,
                        Score = x.score.ToString(),
                        DateText = x.completed_at.HasValue
                            ? x.completed_at.Value.ToString("dd.MM.yyyy")
                            : "—"
                    }).ToList();

                    ProgressGrid.ItemsSource = list;
                    ProgressSummaryText.Text = list.Any()
                        ? "Всего записей: " + list.Count
                        : "Прогресс пока пуст. Начните проходить уроки!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки прогресса: " + ex.Message);
            }
        }

        private void LoadAchievements()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    int userId = MainWindow.CurrentUser.id;
                    var earned = db.user_achievements.Where(ua => ua.user_id == userId).ToList();

                    var list = db.achievements.ToList().Select(a =>
                    {
                        var ua = earned.FirstOrDefault(x => x.achievement_id == a.id);
                        return new AchievementItem
                        {
                            Name = a.name,
                            Description = a.description ?? "",
                            DateText = ua != null
                                ? "Получено: " + ua.date_received.ToString("dd.MM.yyyy")
                                : "Ещё не получено",
                            Opacity = ua != null ? 1.0 : 0.45
                        };
                    }).ToList();

                    AchievementsList.ItemsSource = list;
                    NoAchievementsText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки достижений: " + ex.Message);
            }
        }

        private void LoadMyReviews()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    int userId = MainWindow.CurrentUser.id;

                    var raw = (from r in db.reviews
                               join c in db.courses on r.course_id equals c.id
                               where r.user_id == userId
                               orderby r.review_date descending
                               select new
                               {
                                   CourseTitle = c.title,
                                   r.rating,
                                   r.comment,
                                   r.review_date
                               }).ToList();

                    var list = raw.Select(x => new ReviewItem
                    {
                        CourseTitle = x.CourseTitle,
                        Stars = new string('★', x.rating) + new string('☆', 5 - x.rating),
                        Comment = string.IsNullOrWhiteSpace(x.comment) ? "Без комментария" : x.comment,
                        DateText = x.review_date.ToString("dd.MM.yyyy")
                    }).ToList();

                    MyReviewsList.ItemsSource = list;
                    NoReviewsText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки отзывов: " + ex.Message);
            }
        }

        // ===================== ПРЕПОДАВАТЕЛЬ =====================
        private void LoadTeacherCourses()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    int teacherId = MainWindow.CurrentUser.id;

                    var list = db.courses
                        .Where(c => c.teacher_id == teacherId)
                        .ToList()
                        .Select(c => new TeacherCourseItem
                        {
                            CourseId = c.id,
                            Title = c.title,
                            StudentsText = db.enrollments.Count(e => e.course_id == c.id) + " студентов",
                            StatusText = "● Активен"
                        }).ToList();

                    TeacherCoursesList.ItemsSource = list;
                    NoTeacherCoursesText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки курсов: " + ex.Message);
            }
        }

        private void LoadStudents()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    int teacherId = MainWindow.CurrentUser.id;

                    var raw = (from e in db.enrollments
                               join c in db.courses on e.course_id equals c.id
                               join u in db.users on e.user_id equals u.id
                               where c.teacher_id == teacherId
                               orderby e.enrollment_date descending
                               select new
                               {
                                   FullName = u.full_name,
                                   CourseTitle = c.title,
                                   e.enrollment_date,
                                   e.status
                               }).ToList();

                    var list = raw.Select(x => new StudentItem
                    {
                        FullName = x.FullName,
                        CourseTitle = x.CourseTitle,
                        DateText = x.enrollment_date.ToString("dd.MM.yyyy"),
                        Status = x.status
                    }).ToList();

                    StudentsGrid.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки студентов: " + ex.Message);
            }
        }

        // ===================== АДМИН =====================
        private void LoadUsers(string search = "")
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

        private void LoadAllCourses()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    // Сначала выгружаем в память, потом форматируем (ToString не переводится в SQL)
                    var raw = (from c in db.courses
                               join u in db.users on c.teacher_id equals u.id
                               select new
                               {
                                   c.id,
                                   c.title,
                                   Teacher = u.full_name,
                                   c.level,
                                   c.price
                               }).ToList();

                    var list = raw.Select(x => new CourseAdminItem
                    {
                        Id = x.id,
                        Title = x.title,
                        Teacher = x.Teacher,
                        Level = x.level,
                        PriceText = x.price.ToString("N0") + " ₽"
                    }).ToList();

                    AllCoursesGrid.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки курсов: " + ex.Message);
            }
        }

        private void LoadModeration()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    // IsNullOrWhiteSpace / ToString тоже нельзя внутри LINQ to Entities
                    var raw = (from r in db.reviews
                               join c in db.courses on r.course_id equals c.id
                               join u in db.users on r.user_id equals u.id
                               orderby r.review_date descending
                               select new
                               {
                                   r.id,
                                   UserName = u.full_name,
                                   CourseTitle = c.title,
                                   r.rating,
                                   r.comment,
                                   r.review_date
                               }).ToList();

                    var list = raw.Select(x => new ModerationItem
                    {
                        ReviewId = x.id,
                        Header = x.UserName + " → " + x.CourseTitle + "  (" + x.rating + "/5)",
                        Comment = string.IsNullOrWhiteSpace(x.comment) ? "Без комментария" : x.comment,
                        DateText = x.review_date.ToString("dd.MM.yyyy")
                    }).ToList();

                    ModerationList.ItemsSource = list;
                    NoModerationText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки отзывов: " + ex.Message);
            }
        }

        private void LoadAdminStats()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    AdminStatsText.Text =
                        "Пользователей: " + db.users.Count() + "\n" +
                        "Курсов: " + db.courses.Count() + "\n" +
                        "Записей на курсы: " + db.enrollments.Count() + "\n" +
                        "Отзывов: " + db.reviews.Count();
                }
            }
            catch (Exception ex)
            {
                AdminStatsText.Text = "Ошибка: " + ex.Message;
            }
        }

        // ===================== КНОПКИ =====================
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }

        private void ContinueCourse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int courseId)
                NavigationService?.Navigate(new CoursePage(courseId));
        }

        private void CreateCourse_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Создание курса будет добавлено позже.", "В разработке");
        }

        private void EditCourse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                MessageBox.Show("Редактирование курса ID=" + id + " (в разработке)");
        }

        private void FreezeCourse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                MessageBox.Show("Заморозка курса ID=" + id + " (в разработке)");
        }

        private void ManageLessons_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                MessageBox.Show("Управление уроками курса ID=" + id + " (в разработке)");
        }

        private void UserSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadUsers(UserSearchBox.Text.Trim());
        }

        private void UsersGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UsersGrid.SelectedItem is UserItem user)
            {
                MessageBox.Show("Пользователь: " + user.FullName + "\nРоль: " + user.Role +
                                "\n\nСмена роли / удаление — в разработке.");
            }
        }

        private void DeleteReview_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int reviewId)) return;

            if (MessageBox.Show("Удалить этот отзыв?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new KIPLangEntities())
                {
                    var rev = db.reviews.FirstOrDefault(r => r.id == reviewId);
                    if (rev != null)
                    {
                        db.reviews.Remove(rev);
                        db.SaveChanges();
                        LoadModeration();
                        MessageBox.Show("Отзыв удалён.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }
    }
}