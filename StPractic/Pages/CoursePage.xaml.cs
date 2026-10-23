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

                    // Заголовок страницы
                    Title = $"Курс: {_course.title}";

                    // Основные данные
                    CourseTitleText.Text = _course.title;
                    DescriptionText.Text = string.IsNullOrWhiteSpace(_course.description)
                        ? "Описание отсутствует."
                        : _course.description;
                    LanguageText.Text = _course.languages?.name ?? "—";
                    LevelText.Text = _course.level ?? "—";
                    TeacherText.Text = _course.users?.full_name ?? "—";
                    PriceText.Text = $"{_course.price:N0} ₽";
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
                // Только студент может записаться (преподаватель и админ — нет)
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

            // Уже записан?
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
            catch { /* игнорируем, окно оплаты всё равно проверит */ }

            // Открываем окно оплаты
            var payWin = new PaymentWindow(
                _courseId,
                currentUser.id,
                _course?.title ?? "Курс",
                _course?.price ?? 0);

            payWin.Owner = Window.GetWindow(this);

            if (payWin.ShowDialog() == true && payWin.PaymentSuccess)
            {
                LoadCourse(); // обновляем кнопку / бейдж
            }
        }

        private void Lesson_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is LessonItem lesson)
            {
                // Пока просто сообщение — страницу урока сделаем следующим шагом
                MessageBox.Show(
                    $"Открытие урока:\n«{lesson.Title}»\n\n(Страница урока будет добавлена следующим шагом)",
                    "Урок",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // В будущем:
                // NavigationService?.Navigate(new LessonPage(lesson.Id));
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