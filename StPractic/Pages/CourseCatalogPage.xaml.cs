using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StPractic.Pages
{
    public partial class CourseCatalogPage : Page
    {
        // --- 1. Классы данных (вместо анонимных типов) ---
        public class LanguageItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class CourseItem
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Level { get; set; }
            public decimal Price { get; set; }
            public int LanguageId { get; set; }
            public string LanguageName { get; set; }
            public string TeacherName { get; set; }
        }

        private List<CourseItem> _allCourses = new List<CourseItem>();

        public CourseCatalogPage()
        {
            InitializeComponent();
            LoadLanguages();
            LoadCourses();
        }

        private void LoadLanguages()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    // Создаем список для ComboBox
                    var langList = new List<LanguageItem>();

                    // Добавляем пункт "Все языки"
                    langList.Add(new LanguageItem { Id = 0, Name = "Все языки" });

                    // Загружаем реальные языки
                    var dbLangs = db.languages.ToList();
                    foreach (var l in dbLangs)
                    {
                        langList.Add(new LanguageItem { Id = l.id, Name = l.name });
                    }

                    LanguageFilterComboBox.ItemsSource = langList;
                    LanguageFilterComboBox.SelectedIndex = 0; // Выбираем "Все языки"
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки языков: {ex.Message}");
            }
        }

        private void LoadCourses()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    // Загружаем данные из БД в список классов CourseItem
                    _allCourses = (from c in db.courses
                                   join l in db.languages on c.language_id equals l.id
                                   join u in db.users on c.teacher_id equals u.id
                                   select new CourseItem
                                   {
                                       Id = c.id,
                                       Title = c.title,
                                       Level = c.level,
                                       Price = c.price,
                                       LanguageId = l.id,
                                       LanguageName = l.name,
                                       TeacherName = u.full_name
                                   }).ToList();

                    ApplyFilters(); // Сразу применяем фильтры (покажет все)
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курсов из БД: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            if (_allCourses == null) return;

            var filtered = _allCourses.AsEnumerable();

            // 1. Фильтр по тексту
            string search = SearchTextBox?.Text?.Trim()?.ToLower() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(c => c.Title != null && c.Title.ToLower().Contains(search));
            }

            // 2. Фильтр по языку (берем выбранный объект)
            if (LanguageFilterComboBox?.SelectedItem is LanguageItem selectedLang)
            {
                if (selectedLang.Id != 0)
                {
                    filtered = filtered.Where(c => c.LanguageId == selectedLang.Id);
                }
            }

            CoursesList.ItemsSource = filtered.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void LanguageFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CourseDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int courseId)
            {
                MessageBox.Show($"Переход к курсу ID: {courseId}.\n(Страница деталей курса в разработке)");
                // В будущем: NavigationService.Navigate(new CourseDetailsPage(courseId));
            }
        }
    }
}