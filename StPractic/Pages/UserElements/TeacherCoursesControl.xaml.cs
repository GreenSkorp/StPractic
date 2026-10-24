using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages.UserElements
{
    public partial class TeacherCoursesControl : UserControl
    {
        public TeacherCoursesControl()
        {
            InitializeComponent();
            Loaded += (s, e) => Refresh();
        }

        public void Refresh()
        {
            LoadTeacherCourses();
        }

public void LoadTeacherCourses()
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
                            StatusText = (c.description ?? "").Contains("<!--FROZEN-->") ? "● Заморожен" : "● Активен"
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

private void CreateCourse_Click(object sender, RoutedEventArgs e)
        {
            var win = new StPractic.Pages.CourseEditWindow(null);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true && win.Saved)
                LoadTeacherCourses();
        }

private void EditCourse_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int id)) return;
            var win = new StPractic.Pages.CourseEditWindow(id);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true && win.Saved)
                LoadTeacherCourses();
        }

        private void FreezeCourse_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int id)) return;

            try
            {
                using (var db = new KIPLangEntities())
                {
                    var course = db.courses.FirstOrDefault(c => c.id == id);
                    if (course == null) return;

                    string desc = course.description ?? "";
                    bool frozen = desc.Contains("<!--FROZEN-->");

                    if (frozen)
                    {
                        MessageBox.Show("Нельзя заморозить уже замороженный курс.");
                    }
                    else
                    {
                        if (MessageBox.Show(
                                "Заморозить курс «" + course.title + "»?\nНовые записи будут недоступны.",
                                "Заморозка",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) != MessageBoxResult.Yes)
                            return;

                        course.description = desc.TrimEnd() + "\n<!--FROZEN-->";
                        MessageBox.Show("Курс заморожен.");
                    }
                    db.SaveChanges();
                    LoadTeacherCourses();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }


        private void UnFreezeCourse_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int id)) return;

            try
            {
                using (var db = new KIPLangEntities())
                {
                    var course = db.courses.FirstOrDefault(c => c.id == id);
                    if (course == null) return;

                    string desc = course.description ?? "";
                    bool frozen = desc.Contains("<!--FROZEN-->");

                    if (frozen)
                    {
                        course.description = desc.Replace("<!--FROZEN-->", "").Trim();
                        MessageBox.Show("Курс разморожен. Запись снова доступна.");
                    }
                    else
                    {
                        MessageBox.Show("Нельзя разморозить уже активный курс.");

                    }
                    db.SaveChanges();
                    LoadTeacherCourses();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
        private void ManageLessons_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int id)) return;
            string title = null;
            try
            {
                using (var db = new KIPLangEntities())
                {
                    var c = db.courses.FirstOrDefault(x => x.id == id);
                    title = c?.title;
                }
            }
            catch { }

            var win = new StPractic.Pages.ManageLessonsWindow(id, title);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }
    }
}
