using System.Windows;
using log4net;
namespace DellDefense
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        string userName = "preethamwilfredjohn@gmail.com";
        string Password = "DellDefense";
        public StartPage()
        {
            InitializeComponent();
        }
       
        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Validating User Name and Password");
            //compare user name and password
            if (UserNameTextBox.Text == userName)
            {
                if (PasswordTextBox.Text == Password)
                {
                    Application.Current.Properties["email"] = userName;
                    log.Info("User Name and password matches. Navigating to next page");
                    //navigate to next page
                    MainWindow mw = new MainWindow();
                    this.Hide();
                    mw.Show();
                }
            }
            else
            {
                log.Info("Invalid username and password");
                MessageBox.Show("Incorrect username and password");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Cancel button clicked. Quitting the application");
            //close window on cancel button
            this.Close();
        }
    }
}
