using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages.UserElements
{
    public partial class ProgressControl : UserControl
    {
        public ProgressControl()
        {
            InitializeComponent();
            Loaded += (s, e) => Refresh();
        }

        public void Refresh()
        {
            LoadProgress();
        }

public void LoadProgress()
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
    }
}
