using System;

namespace StPractic.Pages.UserElements
{

    public class CourseCardItem
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string LevelInfo { get; set; }
        public double Progress { get; set; }
        public string ProgressText { get; set; }
    }
    public class ProgressItem
    {
        public string CourseTitle { get; set; }
        public string LessonTitle { get; set; }
        public string Status { get; set; }
        public string Score { get; set; }
        public string DateText { get; set; }
    }
    public class AchievementItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DateText { get; set; }
        public double Opacity { get; set; } = 1.0;
    }
    public class ReviewItem
    {
        public string CourseTitle { get; set; }
        public string Stars { get; set; }
        public string Comment { get; set; }
        public string DateText { get; set; }
    }
    public class TeacherCourseItem
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string StudentsText { get; set; }
        public string StatusText { get; set; }
    }
    public class StudentItem
    {
        public string FullName { get; set; }
        public string CourseTitle { get; set; }
        public string DateText { get; set; }
        public string Status { get; set; }
    }
    public class UserItem
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public string DateText { get; set; }
    }
    public class CourseAdminItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Teacher { get; set; }
        public string Level { get; set; }
        public string PriceText { get; set; }
    }
    public class ModerationItem
    {
        public int ReviewId { get; set; }
        public string Header { get; set; }
        public string Comment { get; set; }
        public string DateText { get; set; }
    }
    public class CourseStatItem
    {
        public string Title { get; set; }
        public int Students { get; set; }
        public string StudentsText { get; set; }
        public double BarWidth { get; set; }
        public double Progress { get; set; }
        public string ProgressText { get; set; }
    }

}
