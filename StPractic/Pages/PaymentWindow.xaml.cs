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

        /// <summary>
        /// true — оплата прошла и запись на курс создана
        /// </summary>
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
            // Простая проверка заполнения (имитация)
            if (string.IsNullOrWhiteSpace(CardNumberBox.Text) ||
                string.IsNullOrWhiteSpace(ExpiryBox.Text) ||
                string.IsNullOrWhiteSpace(CvcBox.Text))
            {
                MessageBox.Show("Заполните данные карты.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CardNumberBox.Text.Replace(" ", "").Length < 12)
            {
                MessageBox.Show("Некорректный номер карты.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new KIPLangEntities())
                {
                    // Уже записан?
                    if (db.enrollments.Any(en => en.user_id == _userId && en.course_id == _courseId))
                    {
                        MessageBox.Show("Вы уже записаны на этот курс.", "Информация");
                        PaymentSuccess = true;
                        DialogResult = true;
                        Close();
                        return;
                    }

                    // 1. Платёж (id не указываем — IDENTITY)
                    var payment = new payments
                    {
                        user_id = _userId,
                        course_id = _courseId,
                        amount = _amount,
                        payment_date = DateTime.Today,
                        status = "paid"
                    };
                    db.payments.Add(payment);

                    // 2. Запись на курс (id не указываем — IDENTITY)
                    var enrollment = new enrollments
                    {
                        user_id = _userId,
                        course_id = _courseId,
                        enrollment_date = DateTime.Today,
                        status = "active"
                    };
                    db.enrollments.Add(enrollment);

                    db.SaveChanges();
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