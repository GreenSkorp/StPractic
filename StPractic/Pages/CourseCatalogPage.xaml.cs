using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StPractic.Pages
{
    public partial class CourseCatalogPage : Page
    {
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
                    var langList = new List<LanguageItem>();
                    langList.Add(new LanguageItem { Id = 0, Name = "Все языки" });

                    var dbLangs = db.languages.ToList();
                    foreach (var l in dbLangs)
                    {
                        langList.Add(new LanguageItem { Id = l.id, Name = l.name });
                    }

                    LanguageFilterComboBox.ItemsSource = langList;
                    LanguageFilterComboBox.SelectedIndex = 0;
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

                    ApplyFilters();
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

            string search = SearchTextBox?.Text?.Trim()?.ToLower() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(c => c.Title != null && c.Title.ToLower().Contains(search));
            }

            if (LanguageFilterComboBox?.SelectedItem is LanguageItem selectedLang)
            {
                if (selectedLang.Id != 0)
                {
                    filtered = filtered.Where(c => c.LanguageId == selectedLang.Id);
                }
            }

            // Используем именно CoursesListView вместо CoursesList
            CoursesListView.ItemsSource = filtered.ToList();
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
                NavigationService?.Navigate(new CoursePage(courseId));
            }
        }
    }
}