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
    public partial class CourseEditWindow : Window
    {
        private readonly int? _courseId;
        public bool Saved { get; private set; }

        public CourseEditWindow(int? courseId = null)
        {
            InitializeComponent();
            _courseId = courseId;
            HeaderText.Text = courseId.HasValue ? "Редактирование курса" : "Создание курса";
            LoadLanguages();
            if (courseId.HasValue) LoadCourse(courseId.Value);
        }

        private void LoadLanguages()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    LanguageCombo.ItemsSource = db.languages.OrderBy(l => l.name).ToList();
                    if (LanguageCombo.Items.Count > 0)
                        LanguageCombo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки языков: " + ex.Message);
            }
        }

        private void LoadCourse(int id)
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    var c = db.courses.FirstOrDefault(x => x.id == id);
                    if (c == null) return;

                    TitleBox.Text = c.title;
                    // убираем маркер заморозки при редактировании
                    DescBox.Text = (c.description ?? "").Replace("<!--FROZEN-->", "").Trim();
                    LanguageCombo.SelectedValue = c.language_id;
                    PriceBox.Text = c.price.ToString("0");

                    foreach (ComboBoxItem item in LevelCombo.Items)
                    {
                        if ((item.Content as string) == c.level)
                        {
                            LevelCombo.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки курса: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Введите название курса.");
                return;
            }

            if (LanguageCombo.SelectedValue == null)
            {
                MessageBox.Show("Выберите язык.");
                return;
            }

            if (!decimal.TryParse(PriceBox.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену.");
                return;
            }

            string level = (LevelCombo.SelectedItem as ComboBoxItem)?.Content as string ?? "Beginner";
            int langId = (int)LanguageCombo.SelectedValue;

            try
            {
                using (var db = new KIPLangEntities())
                {
                    if (_courseId.HasValue)
                    {
                        var c = db.courses.FirstOrDefault(x => x.id == _courseId.Value);
                        if (c == null)
                        {
                            MessageBox.Show("Курс не найден.");
                            return;
                        }

                        bool wasFrozen = (c.description ?? "").Contains("<!--FROZEN-->");
                        c.title = TitleBox.Text.Trim();
                        c.description = DescBox.Text.Trim() + (wasFrozen ? "\n<!--FROZEN-->" : "");
                        c.language_id = langId;
                        c.level = level;
                        c.price = price;
                    }
                    else
                    {
                        if (MainWindow.CurrentUser == null)
                        {
                            MessageBox.Show("Не авторизован.");
                            return;
                        }

                        db.courses.Add(new courses
                        {
                            title = TitleBox.Text.Trim(),
                            description = DescBox.Text.Trim(),
                            language_id = langId,
                            teacher_id = MainWindow.CurrentUser.id,
                            level = level,
                            price = price,
                            created_at = DateTime.Today
                        });
                    }

                    db.SaveChanges();
                }

                Saved = true;
                MessageBox.Show("Курс сохранён.", "Успех");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string d = ex.Message;
                if (ex.InnerException != null) d += "\n" + ex.InnerException.Message;
                if (ex.InnerException?.InnerException != null)
                    d += "\n" + ex.InnerException.InnerException.Message;
                MessageBox.Show("Ошибка сохранения:\n" + d);
            }
        }
    }
}