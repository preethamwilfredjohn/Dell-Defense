using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Data.SqlClient;
using System;
using log4net;
using System.Security.Cryptography;
namespace DellDefense
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        CommonOpenFileDialog dialog = new CommonOpenFileDialog();
        SqlConnection con,con1,compareConnection = null;
        Hashing hash = new Hashing();

        public MainWindow()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
            
            

        }

        //choose directory 
        private void PathButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Choose Path button clicked");
            dialog.InitialDirectory = @"S\";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePathTextBox.Text = dialog.FileName;
            }            
        }
        //check connection with database
        private void CheckConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Check Connection button clicked");
            if (!string.IsNullOrWhiteSpace(FilePathTextBox.Text) && !string.IsNullOrWhiteSpace(DataSourceTextBox.Text) && !string.IsNullOrWhiteSpace(DBNameTextBox.Text) && !string.IsNullOrWhiteSpace(DBPasswordBox.Password) && !string.IsNullOrWhiteSpace(UserIDTextBox.Text))
            {
                string connectionString = "Data Source=" + DataSourceTextBox.Text + ";Initial Catalog=" + DBNameTextBox.Text + ";User ID=" + UserIDTextBox.Text + ";Password=" + DBPasswordBox.Password;
                con = new SqlConnection(connectionString);
                try
                {
                    log.Info("Trying to connect to DataBase");
                    con.Open();
                    log.Info("Connected to DataBase");
                    MessageBox.Show("Connection Sccessful");
                    log.Info("Closing DataBase connection");
                    con.Close();
                }
                catch (SqlException ex)
                {
                    log.Error("Connection to Database failed with exception "+ex);
                    MessageBox.Show("Connection not established. Exception " + ex);
                }
            }
            else
            {
                MessageBox.Show("Please enter all the details");
            }
        }

        //loading database with the hash values of the file
        private void LoadDBButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Load To Database button clicked");
            if (!string.IsNullOrWhiteSpace(FilePathTextBox.Text) && !string.IsNullOrWhiteSpace(DataSourceTextBox.Text) && !string.IsNullOrWhiteSpace(DBNameTextBox.Text) && !string.IsNullOrWhiteSpace(DBPasswordBox.Password) && !string.IsNullOrWhiteSpace(UserIDTextBox.Text))
            {
                string connectionString = "Data Source=" + DataSourceTextBox.Text + ";Initial Catalog=" + DBNameTextBox.Text + ";User ID=" + UserIDTextBox.Text + ";Password=" + DBPasswordBox.Password;
                con1 = new SqlConnection(connectionString);
                try
                {                    
                    log.Info("Establishing connection to database");
                    con1.Open();
                    log.Info("Removing pre exsisting data from DataBase");
                    SqlCommand del = new SqlCommand("delete from DellDefenseDB", con1);
                    del.ExecuteNonQuery();
                    //hashing and loading the root directory files
                    foreach (string file in Directory.EnumerateFiles(dialog.FileName))
                    {
                        try
                        {
                            string hashedContent = Hashing.BytesToString(hash.GetHashSha256(file));                            
                            SqlCommand sc = new SqlCommand("INSERT into DellDefenseDB values(@fileName,@fileContent)", con1);
                            sc.Parameters.AddWithValue("fileName", file);
                            sc.Parameters.AddWithValue("fileContent", hashedContent);
                            log.Info("Inserting file details into database " + file);
                            sc.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            log.Error("Error while writing to database" + ex);
                        }
                    }
                    string dir = FilePathTextBox.Text;
                    //hashing and loading the sub directory files
                    foreach (string d in Directory.GetDirectories(dir))
                    {
                        foreach (string f in Directory.GetFiles(d))
                        {
                            try
                            {
                                string hashedContent = Hashing.BytesToString(hash.GetHashSha256(f));
                                SqlCommand sc1 = new SqlCommand("INSERT into DellDefenseDB values(@fileName,@fileContent)", con1);
                                sc1.Parameters.AddWithValue("@fileName", f);
                                sc1.Parameters.AddWithValue("@fileContent", hashedContent);
                                log.Info("Inserting file details {0} into database" + f);
                                sc1.ExecuteNonQuery();
                            }
                            catch (SqlException ex)
                            {
                                log.Error("Error while writing to database" + ex);
                            }
                        }
                    }
                    log.Info("Data loaded to database");
                    MessageBox.Show("Data loaded into Database");
                }
                catch (SqlException ex)
                {
                    log.Error("Connection to Database failed with exception " + ex);
                }
                //closing database connection
                finally
                {
                    con1.Close();
                    log.Info("DataBase Connection closed to database");
                }
            }
            else
            {
                MessageBox.Show("Please enter all the details");
            }
        }
        //starting the application
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartDateTimePicker.Value != null && EndDateTimePicker.Value !=null)
            {
                string email = (string)App.Current.Properties["email"];
                DateTime startTime = (DateTime)StartDateTimePicker.Value;
                DateTime endTime = (DateTime)EndDateTimePicker.Value;
                string dataSource = DataSourceTextBox.Text;
                string dataBase = DBNameTextBox.Text;
                string userName = UserIDTextBox.Text;
                string password = DBPasswordBox.Password;
                string dirPath = dialog.FileName;
                int jobInterval = Convert.ToInt32(JobIntervalTextBox.Text);

                //invoking the function for scheduling the jobs 
                Schedule startApplication = new Schedule();
                log.Info("Start Button Clicked");
                if (!string.IsNullOrWhiteSpace(JobIntervalTextBox.Text) && !string.IsNullOrWhiteSpace(dirPath) && !string.IsNullOrWhiteSpace(dataSource) && !string.IsNullOrWhiteSpace(dataBase) && !string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(userName))
                {
                    string checkD = null;
                    string connectionString = "Data Source=" + dataSource + ";Initial Catalog=" + dataBase + ";User ID=" + userName + ";Password=" + password;
                    compareConnection = new SqlConnection(connectionString);
                    try
                    {
                        log.Info("Trying to connect to DataBase");
                        compareConnection.Open();     
                        //checking if the database has content
                        SqlCommand checkData = new SqlCommand("Select top 1 fileName from DellDefenseDB", compareConnection);
                        SqlDataReader readFirst = checkData.ExecuteReader();
                        while (readFirst.Read())
                        {
                            checkD = (string)readFirst["fileName"];
                        }
                        readFirst.Close();
                        checkData.Dispose();
                        if (string.IsNullOrEmpty(checkD))
                        {
                            MessageBox.Show("No rows in DataBase. Please load data into Database");
                        }
                        else
                        {
                            Hide();
                            MessageBox.Show("Application is running and will be completed by the end time specified");
                            startApplication.Start(startTime, endTime, dataSource, dataBase, userName, password, dirPath, email, jobInterval);
                        }
                    }
                    catch (SqlException ex)
                    {
                        log.Error("Connection to Database failed with exception " + ex);
                    }
                    //closing database connection
                    finally
                    {
                        log.Info("Closing DataBase connection");
                        compareConnection.Close();
                    }                                        
                }
                else
                {
                    MessageBox.Show("Please enter all the details");
                }
            }
            else
            {
                MessageBox.Show("Please enter all the details");
            }
            
        }       
    }
}
