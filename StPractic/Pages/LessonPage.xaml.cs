using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StPractic.Pages
{
    public partial class LessonPage : Page
    {
        private readonly int _lessonId;
        private lessons _lesson;
        private int? _testId;
        private int _exerciseScore;
        private int _testScore;

        public class ExerciseItem : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public string TypeLabel { get; set; }
            public string Question { get; set; }
            public string CorrectAnswer { get; set; }
            public string UserAnswer { get; set; }
            public bool IsCorrect { get; set; }
            public bool Checked { get; set; }

            private string _resultText;
            public string ResultText
            {
                get => _resultText;
                set { _resultText = value; OnPropertyChanged(); }
            }

            private Brush _resultBrush = Brushes.Gray;
            public Brush ResultBrush
            {
                get => _resultBrush;
                set { _resultBrush = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string n = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
        }

        public class AnswerItem : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public string AnswerText { get; set; }
            public bool IsCorrect { get; set; }
            public string GroupName { get; set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string n = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
        }

        public class QuestionItem
        {
            public int Id { get; set; }
            public string QuestionText { get; set; }
            public List<AnswerItem> Answers { get; set; }
        }

        public LessonPage(int lessonId)
        {
            InitializeComponent();
            _lessonId = lessonId;
            LoadLesson();
        }

        private void LoadLesson()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    _lesson = db.lessons.FirstOrDefault(l => l.id == _lessonId);
                    if (_lesson == null)
                    {
                        MessageBox.Show("Урок не найден.");
                        return;
                    }

                    var course = db.courses.FirstOrDefault(c => c.id == _lesson.course_id);
                    CourseNameText.Text = course != null ? course.title : "";
                    LessonTitleText.Text = _lesson.title;
                    LessonMetaText.Text = "Урок №" + _lesson.order_num + "  ·  " + _lesson.duration + " мин";
                    ContentText.Text = string.IsNullOrWhiteSpace(_lesson.content)
                        ? "Материал урока пока не добавлен."
                        : _lesson.content;

                    // Статус прохождения
                    if (MainWindow.CurrentUser != null)
                    {
                        var progress = db.user_progress.FirstOrDefault(p =>
                            p.user_id == MainWindow.CurrentUser.id && p.lesson_id == _lessonId);

                        if (progress != null && progress.status == "completed")
                        {
                            StatusBadge.Visibility = Visibility.Visible;
                            StatusBadge.Background = (Brush)FindResource("BrushSuccess");
                            StatusBadgeText.Text = "Пройден";
                            StatusBadgeText.Foreground = Brushes.White;
                            CompleteBtn.IsEnabled = false;
                            CompleteBtn.Content = "Урок уже пройден";
                            CompleteHintText.Text = "Балл: " + progress.score;
                        }
                    }

                    // Упражнения
                    var exercises = db.exercises.Where(e => e.lesson_id == _lessonId).ToList();
                    var exerciseItems = exercises.Select(e => new ExerciseItem
                    {
                        Id = e.id,
                        TypeLabel = string.IsNullOrEmpty(e.type) ? "Упражнение" : e.type,
                        Question = e.question,
                        CorrectAnswer = e.correct_answer ?? ""
                    }).ToList();

                    ExercisesList.ItemsSource = exerciseItems;
                    NoExercisesText.Visibility = exerciseItems.Any() ? Visibility.Collapsed : Visibility.Visible;
                    if (!exerciseItems.Any()) ExercisesPanel.Visibility = Visibility.Collapsed;

                    // Тест
                    var test = db.tests.FirstOrDefault(t => t.lesson_id == _lessonId);
                    if (test == null)
                    {
                        NoTestsText.Visibility = Visibility.Visible;
                        SubmitTestBtn.Visibility = Visibility.Collapsed;
                        TestsPanel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        _testId = test.id;
                        var questions = db.test_questions.Where(q => q.test_id == test.id).ToList();
                        var qItems = new List<QuestionItem>();

                        foreach (var q in questions)
                        {
                            var answers = db.test_answers.Where(a => a.question_id == q.id).ToList()
                                .Select(a => new AnswerItem
                                {
                                    Id = a.id,
                                    AnswerText = a.answer_text,
                                    IsCorrect = a.is_correct,
                                    GroupName = "q" + q.id
                                }).ToList();

                            qItems.Add(new QuestionItem
                            {
                                Id = q.id,
                                QuestionText = q.question,
                                Answers = answers
                            });
                        }

                        QuestionsList.ItemsSource = qItems;
                        if (!qItems.Any())
                        {
                            NoTestsText.Visibility = Visibility.Visible;
                            SubmitTestBtn.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки урока: " + GetFullError(ex));
            }
        }

        private void CheckExercise_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is ExerciseItem item)) return;

            string user = (item.UserAnswer ?? "").Trim();
            string correct = (item.CorrectAnswer ?? "").Trim();

            item.Checked = true;
            item.IsCorrect = string.Equals(user, correct, StringComparison.OrdinalIgnoreCase);

            if (item.IsCorrect)
            {
                item.ResultText = "Верно!";
                item.ResultBrush = (Brush)FindResource("BrushSuccess");
            }
            else
            {
                item.ResultText = "Неверно. Правильный ответ: " + correct;
                item.ResultBrush = (Brush)FindResource("BrushDanger");
            }

            // Пересчёт баллов по упражнениям
            var list = ExercisesList.ItemsSource as IEnumerable<ExerciseItem>;
            if (list != null)
            {
                int total = list.Count();
                int ok = list.Count(x => x.Checked && x.IsCorrect);
                _exerciseScore = total > 0 ? (int)Math.Round(100.0 * ok / total) : 0;
            }
        }

        private void SubmitTest_Click(object sender, RoutedEventArgs e)
        {
            var questions = QuestionsList.ItemsSource as List<QuestionItem>;
            if (questions == null || !questions.Any()) return;

            int correct = 0;
            foreach (var q in questions)
            {
                var selected = q.Answers.FirstOrDefault(a => a.IsSelected);
                if (selected != null && selected.IsCorrect)
                    correct++;
            }

            _testScore = (int)Math.Round(100.0 * correct / questions.Count);

            TestResultText.Visibility = Visibility.Visible;
            TestResultText.Text = "Результат теста: " + correct + " из " + questions.Count +
                                  " (" + _testScore + "%)";
            TestResultText.Foreground = _testScore >= 60
                ? (Brush)FindResource("BrushSuccess")
                : (Brush)FindResource("BrushDanger");

            if (MainWindow.CurrentUser != null && _testId.HasValue)
            {
                try
                {
                    using (var db = new KIPLangEntities())
                    {
                        db.Database.ExecuteSqlCommand(
                            "INSERT INTO test_results (user_id, test_id, score, result_date) VALUES (@p0, @p1, @p2, @p3)",
                            MainWindow.CurrentUser.id,
                            _testId.Value,
                            _testScore,
                            DateTime.Today);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Тест проверен, но не удалось сохранить результат:\n" + GetFullError(ex));
                }
            }
        }

        private void CompleteLesson_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUser == null)
            {
                MessageBox.Show("Сначала войдите в систему.");
                return;
            }

            int finalScore = _exerciseScore;
            if (_testId.HasValue && TestResultText.Visibility == Visibility.Visible)
                finalScore = (_exerciseScore + _testScore) / 2;
            else if (_testId.HasValue)
            {
                finalScore = _exerciseScore;
            }

            try
            {
                using (var db = new KIPLangEntities())
                {
                    int userId = MainWindow.CurrentUser.id;

                    int exists = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM user_progress WHERE user_id = @p0 AND lesson_id = @p1",
                        userId, _lessonId).FirstOrDefault();

                    if (exists > 0)
                    {
                        db.Database.ExecuteSqlCommand(
                            "UPDATE user_progress SET status = @p0, score = @p1, completed_at = @p2 WHERE user_id = @p3 AND lesson_id = @p4",
                            "completed", finalScore, DateTime.Today, userId, _lessonId);
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand(
                            "INSERT INTO user_progress (user_id, lesson_id, status, score, completed_at) VALUES (@p0, @p1, @p2, @p3, @p4)",
                            userId, _lessonId, "completed", finalScore, DateTime.Today);
                    }

                    TryGrantAchievements(db, userId);
                }

                MessageBox.Show("Урок отмечен как пройденный!\nБалл: " + finalScore,
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                StatusBadge.Visibility = Visibility.Visible;
                StatusBadge.Background = (Brush)FindResource("BrushSuccess");
                StatusBadgeText.Text = "Пройден";
                StatusBadgeText.Foreground = Brushes.White;
                CompleteBtn.IsEnabled = false;
                CompleteBtn.Content = "Урок уже пройден";
                CompleteHintText.Text = "Балл: " + finalScore;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения прогресса:\n" + GetFullError(ex));
            }
        }

        private void TryGrantAchievements(KIPLangEntities db, int userId)
        {
            try
            {
                int completedCount = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM user_progress WHERE user_id = @p0 AND status = @p1",
                    userId, "completed").FirstOrDefault();

                if (completedCount >= 1)
                    GrantIfNeeded(db, userId, "Первый шаг");
                if (completedCount >= 5)
                    GrantIfNeeded(db, userId, "Усердный ученик");
                if (completedCount >= 10)
                    GrantIfNeeded(db, userId, "Марафонец");
            }
            catch
            {
            }
        }

        private void GrantIfNeeded(KIPLangEntities db, int userId, string achievementName)
        {
            var ach = db.achievements.FirstOrDefault(a => a.name == achievementName);
            if (ach == null) return;

            int has = db.Database.SqlQuery<int>(
                "SELECT COUNT(*) FROM user_achievements WHERE user_id = @p0 AND achievement_id = @p1",
                userId, ach.id).FirstOrDefault();
            if (has > 0) return;

            db.Database.ExecuteSqlCommand(
                "INSERT INTO user_achievements (user_id, achievement_id, date_received) VALUES (@p0, @p1, @p2)",
                userId, ach.id, DateTime.Today);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else if (_lesson != null)
                NavigationService?.Navigate(new CoursePage(_lesson.course_id));
        }

        private static string GetFullError(Exception ex)
        {
            string d = ex.Message;
            if (ex.InnerException != null) d += "\n" + ex.InnerException.Message;
            if (ex.InnerException?.InnerException != null)
                d += "\n" + ex.InnerException.InnerException.Message;
            return d;
        }
    }
}