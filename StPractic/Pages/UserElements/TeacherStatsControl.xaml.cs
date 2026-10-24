using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages.UserElements
{
    public partial class TeacherStatsControl : UserControl
    {
        public TeacherStatsControl()
        {
            InitializeComponent();
            Loaded += (s, e) => Refresh();
        }

        public void Refresh()
        {
            LoadTeacherStats();
        }

public void LoadTeacherStats()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    int teacherId = MainWindow.CurrentUser.id;

                    var myCourses = db.courses.Where(c => c.teacher_id == teacherId).ToList();
                    int coursesCount = myCourses.Count;

                    var courseIds = myCourses.Select(c => c.id).ToList();

                    var enrollments = db.enrollments
                        .Where(e => courseIds.Contains(e.course_id))
                        .ToList();

                    int enrollCount = enrollments.Count;
                    int uniqueStudents = enrollments.Select(e => e.user_id).Distinct().Count();

                    int maxStudents = 1;
                    var chartItems = new List<CourseStatItem>();
                    var progressItems = new List<CourseStatItem>();
                    double progressSum = 0;
                    int progressCourses = 0;

                    const double maxBarPx = 320;

                    foreach (var c in myCourses)
                    {
                        int stCount = enrollments.Count(e => e.course_id == c.id);
                        if (stCount > maxStudents) maxStudents = stCount;

                        var lessonIds = db.lessons.Where(l => l.course_id == c.id).Select(l => l.id).ToList();
                        double avgProg = 0;
                        if (lessonIds.Any() && stCount > 0)
                        {
                            var userIds = enrollments.Where(e => e.course_id == c.id).Select(e => e.user_id).Distinct().ToList();
                            double sum = 0;
                            foreach (int uid in userIds)
                            {
                                int done = db.user_progress.Count(p =>
                                    p.user_id == uid && p.status == "completed" && lessonIds.Contains(p.lesson_id));
                                sum += 100.0 * done / lessonIds.Count;
                            }
                            avgProg = Math.Round(sum / userIds.Count);
                            progressSum += avgProg;
                            progressCourses++;
                        }

                        chartItems.Add(new CourseStatItem
                        {
                            Title = c.title,
                            Students = stCount,
                            StudentsText = stCount + " чел."
                        });

                        progressItems.Add(new CourseStatItem
                        {
                            Title = c.title,
                            Progress = avgProg,
                            ProgressText = avgProg + "%"
                        });
                    }

                    // Ширина полос относительно максимума
                    foreach (var item in chartItems)
                    {
                        item.BarWidth = maxStudents > 0
                            ? Math.Max(4, maxBarPx * item.Students / maxStudents)
                            : 4;
                    }

                    StatCoursesCount.Text = coursesCount.ToString();
                    StatStudentsCount.Text = uniqueStudents.ToString();
                    StatEnrollCount.Text = enrollCount.ToString();
                    StatAvgProgress.Text = progressCourses > 0
                        ? Math.Round(progressSum / progressCourses) + "%"
                        : "0%";

                    CourseStatsChart.ItemsSource = chartItems;
                    ProgressStatsChart.ItemsSource = progressItems;
                    NoTeacherStatsText.Visibility = chartItems.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки статистики: " + ex.Message);
            }
        }
    }
}
