using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages
{
    public partial class CoursePage : Page
    {
        private readonly int _courseId;
        private courses _course;
        private bool _isEnrolled;

        // Вспомогательные классы для привязки
        public class LessonItem
        {
            public int Id { get; set; }
            public int OrderNum { get; set; }
            public string Title { get; set; }
            public string DurationText { get; set; }
        }

        public class ReviewItem
        {
            public string AuthorName { get; set; }
            public string Stars { get; set; }
            public string Comment { get; set; }
            public string DateText { get; set; }
        }

        public CoursePage(int courseId)
        {
            InitializeComponent();
            _courseId = courseId;
            LoadCourse();
        }

        private void LoadCourse()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    _course = db.courses
                        .Include("languages")
                        .Include("users")
                        .FirstOrDefault(c => c.id == _courseId);

                    if (_course == null)
                    {
                        MessageBox.Show("Курс не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        NavigationService?.GoBack();
                        return;
                    }

                    Title = $"Курс: {_course.title}";

                    CourseTitleText.Text = _course.title;
                    DescriptionText.Text = string.IsNullOrWhiteSpace(_course.description)
                        ? "Описание отсутствует."
                        : (_course.description ?? "").Replace("<!--FROZEN-->", "").Trim();
                    LanguageText.Text = _course.languages?.name ?? "—";
                    LevelText.Text = _course.level ?? "—";
                    TeacherText.Text = _course.users?.full_name ?? "—";
                    PriceText.Text = "$" + _course.price.ToString("N0");
                    CreatedAtText.Text = $"Создан: {_course.created_at:dd.MM.yyyy}";

                    // Общая длительность
                    var lessons = db.lessons
                        .Where(l => l.course_id == _courseId)
                        .OrderBy(l => l.order_num)
                        .ToList();

                    int totalMinutes = lessons.Sum(l => l.duration);
                    DurationText.Text = totalMinutes > 0
                        ? (totalMinutes >= 60
                            ? $"{totalMinutes / 60} ч {totalMinutes % 60} мин"
                            : $"{totalMinutes} мин")
                        : "—";

                    // Уроки
                    if (lessons.Any())
                    {
                        LessonsList.ItemsSource = lessons.Select(l => new LessonItem
                        {
                            Id = l.id,
                            OrderNum = l.order_num,
                            Title = l.title,
                            DurationText = l.duration > 0 ? $"{l.duration} мин" : ""
                        }).ToList();
                        NoLessonsText.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        LessonsList.ItemsSource = null;
                        NoLessonsText.Visibility = Visibility.Visible;
                    }

                    // Отзывы + рейтинг
                    var reviews = db.reviews
                        .Where(r => r.course_id == _courseId)
                        .OrderByDescending(r => r.review_date)
                        .ToList();

                    if (reviews.Any())
                    {
                        double avg = reviews.Average(r => r.rating);
                        RatingText.Text = $"★ {avg:F1} ({reviews.Count})";

                        ReviewsList.ItemsSource = reviews.Select(r =>
                        {
                            var author = db.users.FirstOrDefault(u => u.id == r.user_id);
                            return new ReviewItem
                            {
                                AuthorName = author?.full_name ?? "Аноним",
                                Stars = new string('★', r.rating) + new string('☆', 5 - r.rating),
                                Comment = string.IsNullOrWhiteSpace(r.comment) ? "Без комментария" : r.comment,
                                DateText = r.review_date.ToString("dd.MM.yyyy")
                            };
                        }).ToList();

                        ReviewsCountText.Text = $"{reviews.Count} отзыв(ов)";
                        NoReviewsText.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        RatingText.Text = "★ —";
                        ReviewsList.ItemsSource = null;
                        ReviewsCountText.Text = "0 отзывов";
                        NoReviewsText.Visibility = Visibility.Visible;
                    }

                    // Статус записи
                    UpdateEnrollmentStatus(db);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курса: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateEnrollmentStatus(KIPLangEntities db)
        {
            var currentUser = MainWindow.CurrentUser;

            if (currentUser == null)
            {
                EnrollBtn.Visibility = Visibility.Collapsed;
                EnrolledBadge.Visibility = Visibility.Collapsed;
                LoginHintText.Visibility = Visibility.Visible;
                ReviewFormPanel.Visibility = Visibility.Collapsed;
                AlreadyReviewedText.Visibility = Visibility.Collapsed;
                return;
            }

            LoginHintText.Visibility = Visibility.Collapsed;

            _isEnrolled = db.enrollments.Any(e =>
                e.user_id == currentUser.id &&
                e.course_id == _courseId &&
                e.status != "cancelled");

            if (_isEnrolled)
            {
                EnrollBtn.Visibility = Visibility.Collapsed;
                EnrolledBadge.Visibility = Visibility.Visible;
            }
            else
            {
                if (currentUser.role == "User" || currentUser.role == "Студент" ||
                    string.IsNullOrEmpty(currentUser.role))
                {
                    EnrollBtn.Visibility = Visibility.Visible;
                    EnrollBtn.Content = "Записаться на курс";
                    EnrollBtn.IsEnabled = true;
                }
                else
                {
                    EnrollBtn.Visibility = Visibility.Collapsed;
                }
                EnrolledBadge.Visibility = Visibility.Collapsed;
            }

            UpdateReviewForm(db);
        }

        private void EnrollBtn_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = MainWindow.CurrentUser;
            if (currentUser == null)
            {
                MessageBox.Show("Сначала войдите в систему.", "Требуется авторизация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (var db = new KIPLangEntities())
                {
                    if (db.enrollments.Any(en =>
                        en.user_id == currentUser.id && en.course_id == _courseId))
                    {
                        MessageBox.Show("Вы уже записаны на этот курс.", "Информация");
                        LoadCourse();
                        return;
                    }
                }
            }
            catch { }

            var payWin = new PaymentWindow(
                _courseId,
                currentUser.id,
                _course?.title ?? "Курс",
                _course?.price ?? 0);

            payWin.Owner = Window.GetWindow(this);

            if (payWin.ShowDialog() == true && payWin.PaymentSuccess)
            {
                LoadCourse(); 
            }
        }

        private void Lesson_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is LessonItem lesson)
            {
                var user = MainWindow.CurrentUser;
                if (user == null)
                {
                    MessageBox.Show("Сначала войдите в систему.");
                    return;
                }

                string role = (user.role ?? "").ToLower();
                bool isStaff = role == "teacher" || role == "преподаватель" ||
                               role == "admin" || role == "администратор";

                if (!_isEnrolled && !isStaff)
                {
                    MessageBox.Show("Сначала запишитесь на курс, чтобы открывать уроки.",
                        "Нужна запись", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                NavigationService?.Navigate(new LessonPage(lesson.Id));
            }
        }


        private void UpdateReviewForm(KIPLangEntities db)
        {
            ReviewFormPanel.Visibility = Visibility.Collapsed;
            AlreadyReviewedText.Visibility = Visibility.Collapsed;

            var user = MainWindow.CurrentUser;
            if (user == null || !_isEnrolled) return;

            bool already = db.reviews.Any(r => r.user_id == user.id && r.course_id == _courseId);
            if (already)
            {
                AlreadyReviewedText.Visibility = Visibility.Visible;
            }
            else
            {
                ReviewFormPanel.Visibility = Visibility.Visible;
            }
        }

        private void SubmitReview_Click(object sender, RoutedEventArgs e)
        {
            var user = MainWindow.CurrentUser;
            if (user == null)
            {
                MessageBox.Show("Сначала войдите в систему.");
                return;
            }

            if (!_isEnrolled)
            {
                MessageBox.Show("Оставлять отзыв могут только записанные на курс студенты.");
                return;
            }

            int rating = 5;
            if (RatingCombo.SelectedItem is System.Windows.Controls.ComboBoxItem item &&
                int.TryParse(item.Content as string, out int r))
                rating = r;

            string comment = (ReviewCommentBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(comment))
            {
                MessageBox.Show("Напишите текст отзыва.", "Внимание");
                return;
            }

            if (comment.Length < 5)
            {
                MessageBox.Show("Отзыв слишком короткий (минимум 5 символов).", "Внимание");
                return;
            }

            try
            {
                using (var db = new KIPLangEntities())
                {
                    if (db.reviews.Any(x => x.user_id == user.id && x.course_id == _courseId))
                    {
                        MessageBox.Show("Вы уже оставляли отзыв на этот курс.");
                        LoadCourse();
                        return;
                    }

                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO reviews (user_id, course_id, rating, comment, review_date) VALUES (@p0, @p1, @p2, @p3, @p4)",
                        user.id, _courseId, rating, comment, DateTime.Today);
                }

                MessageBox.Show("Спасибо! Ваш отзыв опубликован.", "Успех");
                ReviewCommentBox.Clear();
                LoadCourse();
            }
            catch (Exception ex)
            {
                string d = ex.Message;
                if (ex.InnerException != null) d += "\n" + ex.InnerException.Message;
                if (ex.InnerException?.InnerException != null) d += "\n" + ex.InnerException.InnerException.Message;
                MessageBox.Show("Ошибка сохранения отзыва:\n" + d);
            }
        }


        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                NavigationService?.Navigate(new CourseCatalogPage());
        }
    }
}