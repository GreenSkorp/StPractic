using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages.UserElements
{
    public partial class MyReviewsControl : UserControl
    {
        public MyReviewsControl()
        {
            InitializeComponent();
            Loaded += (s, e) => Refresh();
        }

        public void Refresh()
        {
            LoadMyReviews();
        }

public void LoadMyReviews()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    int userId = MainWindow.CurrentUser.id;

                    var raw = (from r in db.reviews
                               join c in db.courses on r.course_id equals c.id
                               where r.user_id == userId
                               orderby r.review_date descending
                               select new
                               {
                                   CourseTitle = c.title,
                                   r.rating,
                                   r.comment,
                                   r.review_date
                               }).ToList();

                    var list = raw.Select(x => new ReviewItem
                    {
                        CourseTitle = x.CourseTitle,
                        Stars = new string('★', x.rating) + new string('☆', 5 - x.rating),
                        Comment = string.IsNullOrWhiteSpace(x.comment) ? "Без комментария" : x.comment,
                        DateText = x.review_date.ToString("dd.MM.yyyy")
                    }).ToList();

                    MyReviewsList.ItemsSource = list;
                    NoReviewsText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки отзывов: " + ex.Message);
            }
        }
    }
}
