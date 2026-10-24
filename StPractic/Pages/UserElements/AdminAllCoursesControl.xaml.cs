using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages.UserElements
{
    public partial class AdminAllCoursesControl : UserControl
    {
        public AdminAllCoursesControl()
        {
            InitializeComponent();
            Loaded += (s, e) => Refresh();
        }

        public void Refresh()
        {
            LoadAllCourses();
        }

public void LoadAllCourses()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    var raw = (from c in db.courses
                               join u in db.users on c.teacher_id equals u.id
                               select new
                               {
                                   c.id,
                                   c.title,
                                   Teacher = u.full_name,
                                   c.level,
                                   c.price
                               }).ToList();

                    var list = raw.Select(x => new CourseAdminItem
                    {
                        Id = x.id,
                        Title = x.title,
                        Teacher = x.Teacher,
                        Level = x.level,
                        PriceText = "$" + x.price.ToString("N0")
                    }).ToList();

                    AllCoursesGrid.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки курсов: " + ex.Message);
            }
        }
    }
}
