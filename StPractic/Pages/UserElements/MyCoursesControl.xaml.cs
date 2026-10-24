using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages.UserElements
{
    public partial class MyCoursesControl : UserControl
    {
        public MyCoursesControl()
        {
            InitializeComponent();
            Loaded += (s, e) => Refresh();
        }

        public void Refresh()
        {
            LoadMyCourses();
        }

public void LoadMyCourses()
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

private void ContinueCourse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int courseId)
                System.Windows.Navigation.NavigationService.GetNavigationService(this)?.Navigate(new StPractic.Pages.CoursePage(courseId));
        }
    }
}
