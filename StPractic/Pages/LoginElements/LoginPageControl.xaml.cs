using System.Windows;
using System.Windows.Controls;

namespace StPractic.Pages.LoginElements
{
    public partial class LoginPageControl : UserControl
    {
        public LoginPageControl()
        {
            InitializeComponent();
        }

        public string GetLoginText()
        {
            return LoginTextBox.Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // логика входа
        }
    }
}