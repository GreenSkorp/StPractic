using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StPractic.Pages.UserElements
{
    public partial class AchievementsControl : UserControl
    {
        public AchievementsControl()
        {
            InitializeComponent();
            Loaded += (s, e) => Refresh();
        }

        public void Refresh()
        {
            LoadAchievements();
        }

public void LoadAchievements()
        {
            try
            {
                using (var db = new KIPLangEntities())
                {
                    int userId = MainWindow.CurrentUser.id;
                    var earned = db.user_achievements.Where(ua => ua.user_id == userId).ToList();

                    var list = db.achievements.ToList().Select(a =>
                    {
                        var ua = earned.FirstOrDefault(x => x.achievement_id == a.id);
                        return new AchievementItem
                        {
                            Name = a.name,
                            Description = a.description ?? "",
                            DateText = ua != null
                                ? "Получено: " + ua.date_received.ToString("dd.MM.yyyy")
                                : "Ещё не получено",
                            Opacity = ua != null ? 1.0 : 0.45
                        };
                    }).ToList();

                    AchievementsList.ItemsSource = list;
                    NoAchievementsText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки достижений: " + ex.Message);
            }
        }
    }
}
