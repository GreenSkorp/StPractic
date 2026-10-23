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
    public partial class PaymentWindow : Window
    {
        private readonly int _courseId;
        private readonly int _userId;
        private readonly decimal _amount;
        private readonly string _courseTitle;

        public bool PaymentSuccess { get; private set; }

        public PaymentWindow(int courseId, int userId, string courseTitle, decimal amount)
        {
            InitializeComponent();

            _courseId = courseId;
            _userId = userId;
            _courseTitle = courseTitle ?? "Курс";
            _amount = amount;

            CourseTitleText.Text = _courseTitle;
            PriceText.Text = "$" + _amount.ToString("N0");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            PaymentSuccess = false;
            DialogResult = false;
            Close();
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            var errors = new System.Collections.Generic.List<string>();

            string cardRaw = (CardNumberBox.Text ?? "").Replace(" ", "").Replace("-", "");
            string expiry = (ExpiryBox.Text ?? "").Trim();
            string cvc = (CvcBox.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(cardRaw))
                errors.Add("• Номер карты не заполнен");
            else if (!cardRaw.All(char.IsDigit))
                errors.Add("• Номер карты должен содержать только цифры");
            else if (cardRaw.Length < 13 || cardRaw.Length > 19)
                errors.Add("• Номер карты: нужно от 13 до 19 цифр (сейчас " + cardRaw.Length + ")");

            if (string.IsNullOrWhiteSpace(expiry))
                errors.Add("• Срок действия карты не заполнен");
            else if (expiry.Length != 5 || expiry[2] != '/')
                errors.Add("• Срок действия: формат ММ/ГГ (например 12/28)");
            else
            {
                string mm = expiry.Substring(0, 2);
                string yy = expiry.Substring(3, 2);
                if (!int.TryParse(mm, out int month) || month < 1 || month > 12)
                    errors.Add("• Месяц срока действия должен быть от 01 до 12");
                if (!int.TryParse(yy, out int year))
                    errors.Add("• Год срока действия указан неверно");
                else
                {
                    // 20YY
                    int fullYear = 2000 + year;
                    var expDate = new DateTime(fullYear, month, DateTime.DaysInMonth(fullYear, month));
                    if (expDate < DateTime.Today)
                        errors.Add("• Срок действия карты истёк");
                }
            }

            if (string.IsNullOrWhiteSpace(cvc))
                errors.Add("• CVC-код не заполнен");
            else if (!cvc.All(char.IsDigit))
                errors.Add("• CVC должен содержать только цифры");
            else if (cvc.Length < 3 || cvc.Length > 4)
                errors.Add("• CVC: нужно 3 или 4 цифры (сейчас " + cvc.Length + ")");

            if (errors.Count > 0)
            {
                MessageBox.Show(
                    "Проверьте данные карты:\n\n" + string.Join("\n", errors),
                    "Неверный ввод",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new KIPLangEntities())
                {
                    if (db.enrollments.Any(en => en.user_id == _userId && en.course_id == _courseId))
                    {
                        MessageBox.Show("Вы уже записаны на этот курс.", "Информация");
                        PaymentSuccess = true;
                        DialogResult = true;
                        Close();
                        return;
                    }

                    // INSERT без id — корректно с IDENTITY
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO payments (user_id, course_id, amount, payment_date, status) VALUES (@p0, @p1, @p2, @p3, @p4)",
                        _userId, _courseId, _amount, DateTime.Today, "paid");

                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO enrollments (user_id, course_id, enrollment_date, status) VALUES (@p0, @p1, @p2, @p3)",
                        _userId, _courseId, DateTime.Today, "active");
                }

                PaymentSuccess = true;
                MessageBox.Show(
                    "Оплата прошла успешно!\nВы записаны на курс «" + _courseTitle + "».",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string details = ex.Message;
                if (ex.InnerException != null)
                    details += "\n\n" + ex.InnerException.Message;
                if (ex.InnerException?.InnerException != null)
                    details += "\n\n" + ex.InnerException.InnerException.Message;

                MessageBox.Show("Ошибка оплаты:\n" + details, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}