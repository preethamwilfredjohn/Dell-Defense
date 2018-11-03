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

namespace DellDefense
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Window
    {
        string userName = "DellDefense";
        string Password = "DellDefense";
        public StartPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //close window on cancel button
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //compare user name and password
            if (UserNameTextBox.Text == userName)
            {
                if(PasswordTextBox.Text == Password)
                {
                    //navigate to next page
                    MainWindow mw = new MainWindow();
                    this.Hide();
                    mw.Show();
                }
            }
            else
            {
                MessageBox.Show("Incorrect username and password");
            }
        }
    }
}
