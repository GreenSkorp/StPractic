using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages.UserElements
{
    public partial class TeacherStudentsControl : UserControl
    {
        public TeacherStudentsControl()
        {
            InitializeComponent();
            Loaded += (s, e) => Refresh();
        }

        public void Refresh()
        {
            LoadStudents();
        }

public void LoadStudents()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    int teacherId = MainWindow.CurrentUser.id;

                    var raw = (from e in db.enrollments
                               join c in db.courses on e.course_id equals c.id
                               join u in db.users on e.user_id equals u.id
                               where c.teacher_id == teacherId
                               orderby e.enrollment_date descending
                               select new
                               {
                                   FullName = u.full_name,
                                   CourseTitle = c.title,
                                   e.enrollment_date,
                                   e.status
                               }).ToList();

                    var list = raw.Select(x => new StudentItem
                    {
                        FullName = x.FullName,
                        CourseTitle = x.CourseTitle,
                        DateText = x.enrollment_date.ToString("dd.MM.yyyy"),
                        Status = x.status
                    }).ToList();

                    StudentsGrid.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки студентов: " + ex.Message);
            }
        }
    }
}
