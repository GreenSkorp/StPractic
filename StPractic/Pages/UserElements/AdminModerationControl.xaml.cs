using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages.UserElements
{
    public partial class AdminModerationControl : UserControl
    {
        public AdminModerationControl()
        {
            InitializeComponent();
            Loaded += (s, e) => Refresh();
        }

        public void Refresh()
        {
            LoadModeration();
        }

public void LoadModeration()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    var raw = (from r in db.reviews
                               join c in db.courses on r.course_id equals c.id
                               join u in db.users on r.user_id equals u.id
                               orderby r.review_date descending
                               select new
                               {
                                   r.id,
                                   UserName = u.full_name,
                                   CourseTitle = c.title,
                                   r.rating,
                                   r.comment,
                                   r.review_date
                               }).ToList();

                    var list = raw.Select(x => new ModerationItem
                    {
                        ReviewId = x.id,
                        Header = x.UserName + " → " + x.CourseTitle + "  (" + x.rating + "/5)",
                        Comment = string.IsNullOrWhiteSpace(x.comment) ? "Без комментария" : x.comment,
                        DateText = x.review_date.ToString("dd.MM.yyyy")
                    }).ToList();

                    ModerationList.ItemsSource = list;
                    NoModerationText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки отзывов: " + ex.Message);
            }
        }

private void DeleteReview_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int reviewId)) return;

            if (MessageBox.Show("Удалить этот отзыв?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new KIPLangEntities())
                {
                    var rev = db.reviews.FirstOrDefault(r => r.id == reviewId);
                    if (rev != null)
                    {
                        db.reviews.Remove(rev);
                        db.SaveChanges();
                        LoadModeration();
                        MessageBox.Show("Отзыв удалён.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }
    }
}
