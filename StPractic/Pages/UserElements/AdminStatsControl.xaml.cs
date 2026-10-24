using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages.UserElements
{
    public partial class AdminStatsControl : UserControl
    {
        public AdminStatsControl()
        {
            InitializeComponent();
            Loaded += (s, e) => Refresh();
        }

        public void Refresh()
        {
            LoadAdminStats();
        }

public void LoadAdminStats()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    AdminStatsText.Text =
                        "Пользователей: " + db.users.Count() + "\n" +
                        "Курсов: " + db.courses.Count() + "\n" +
                        "Записей на курсы: " + db.enrollments.Count() + "\n" +
                        "Отзывов: " + db.reviews.Count();
                }
            }
            catch (Exception ex)
            {
                AdminStatsText.Text = "Ошибка: " + ex.Message;
            }
        }
    }
}
