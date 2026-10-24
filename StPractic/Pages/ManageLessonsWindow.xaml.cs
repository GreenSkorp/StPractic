using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StPractic.Pages
{
    public partial class ManageLessonsWindow : Window
    {
        private readonly int _courseId;

        public class LessonRow
        {
            public int Id { get; set; }
            public int OrderNum { get; set; }
            public string Title { get; set; }
            public string DurationText { get; set; }
        }

        public ManageLessonsWindow(int courseId, string courseTitle = null)
        {
            InitializeComponent();
            _courseId = courseId;
            HeaderText.Text = string.IsNullOrEmpty(courseTitle)
                ? "Уроки курса"
                : "Уроки: " + courseTitle;
            LoadLessons();
        }

        private void LoadLessons()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    var list = db.lessons
                        .Where(l => l.course_id == _courseId)
                        .OrderBy(l => l.order_num)
                        .ToList()
                        .Select(l => new LessonRow
                        {
                            Id = l.id,
                            OrderNum = l.order_num,
                            Title = l.title,
                            DurationText = l.duration + " мин"
                        }).ToList();

                    LessonsGrid.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void AddLesson_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewTitleBox.Text))
            {
                MessageBox.Show("Введите название урока.");
                return;
            }

            if (!int.TryParse(NewDurationBox.Text, out int duration) || duration <= 0)
                duration = 30;

            try
            {
                using (var db = new KIPLangEntities())
                {
                    int nextOrder = 1;
                    if (db.lessons.Any(l => l.course_id == _courseId))
                        nextOrder = db.lessons.Where(l => l.course_id == _courseId).Max(l => l.order_num) + 1;

                    // INSERT без id — таблица lessons с IDENTITY
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO lessons (course_id, title, content, order_num, duration) VALUES (@p0, @p1, @p2, @p3, @p4)",
                        _courseId,
                        NewTitleBox.Text.Trim(),
                        NewContentBox.Text.Trim(),
                        nextOrder,
                        duration);
                }

                NewTitleBox.Text = "";
                NewContentBox.Text = "";
                NewDurationBox.Text = "30";
                LoadLessons();
                MessageBox.Show("Урок добавлен.");
            }
            catch (Exception ex)
            {
                string d = ex.Message;
                if (ex.InnerException != null) d += "\n" + ex.InnerException.Message;
                if (ex.InnerException?.InnerException != null)
                    d += "\n" + ex.InnerException.InnerException.Message;
                MessageBox.Show("Ошибка:\n" + d);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}