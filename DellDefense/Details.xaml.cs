using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Data.SqlClient;
using System;
namespace DellDefense
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CommonOpenFileDialog dialog = new CommonOpenFileDialog();
        SqlConnection con,con1,compareConnection = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PathButton_Click(object sender, RoutedEventArgs e)
        {            
            dialog.InitialDirectory = @"C:\Users\preet\Desktop\Test";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePathTextBox.Text = dialog.FileName;
            }            
        }

        private void CheckConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Data Source=" + DataSourceTextBox.Text + ";Initial Catalog=" + DBNameTextBox.Text + ";User ID=" + UserIDTextBox.Text + ";Password=" + PasswordTextBox.Text;
            con = new SqlConnection(connectionString);
            try
            {
                con.Open();
                MessageBox.Show("Connection Sccessful");
                con.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Connection not established. Exception "+ex );
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Data Source=" + DataSourceTextBox.Text + ";Initial Catalog=" + DBNameTextBox.Text + ";User ID=" + UserIDTextBox.Text + ";Password=" + PasswordTextBox.Text;
            compareConnection = new SqlConnection(connectionString);
            try
            {
                compareConnection.Open();
                foreach (string file in Directory.EnumerateFiles(dialog.FileName))
                {
                    string content = File.ReadAllText(file);
                    string fileContentDB = null;
                    try
                    {
                        using (SqlCommand sc = new SqlCommand("select fileContent from DellDefenseDB where fileName = @fileName", compareConnection))
                        {
                            sc.Parameters.AddWithValue("@fileName", file);
                            using (SqlDataReader readDB = sc.ExecuteReader())
                            {
                                while (readDB.Read())
                                {
                                    fileContentDB = (string)readDB["fileContent"];
                                }
                            }
                                
                        }                           
                        if (content != fileContentDB)
                        {
                            MessageBox.Show("Code has been modified. Please validate");
                        }
                        else
                        {
                            MessageBox.Show("Validation Successfull root directory");
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("error reading file from database"+ex);
                    }
                }
                string dir = FilePathTextBox.Text;
                foreach (string d in Directory.GetDirectories(dir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        string fileContentDB1 = null;
                        string content1 = File.ReadAllText(f);
                        try
                        {
                            using (SqlCommand sc1 = new SqlCommand("select fileContent from DellDefenseDB where fileName = @fileName", compareConnection))
                            {
                                sc1.Parameters.AddWithValue("@fileName", f);
                                using(SqlDataReader readDB1 = sc1.ExecuteReader())
                                {
                                    while (readDB1.Read())
                                    {
                                        fileContentDB1 = (string)readDB1["fileContent"];
                                    }
                                }                                                                
                            }                                
                            if (content1 != fileContentDB1)
                            {
                                MessageBox.Show("Code has been modified. Please validate");
                            }
                            else
                            {
                                MessageBox.Show("Validation Successfull sub directory");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error in writing to database" + ex);
                        }
                    }
                }
            }
            finally
            {
                compareConnection.Close();
            }            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Data Source=" + DataSourceTextBox.Text + ";Initial Catalog=" + DBNameTextBox.Text + ";User ID=" + UserIDTextBox.Text + ";Password=" + PasswordTextBox.Text;
            con1 = new SqlConnection(connectionString);
            try
            {
                con1.Open();
                foreach (string file in Directory.EnumerateFiles(dialog.FileName))
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        SqlCommand sc = new SqlCommand("INSERT into DellDefenseDB values(@fileName,@fileContent)",con1);
                        sc.Parameters.AddWithValue("@fileName", file);
                        sc.Parameters.AddWithValue("@fileContent", content);
                        sc.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error in writing to database" + ex);
                    }
                }
                string dir = FilePathTextBox.Text;
                foreach (string d in Directory.GetDirectories(dir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        try
                        {
                            string content1 = File.ReadAllText(f);
                            SqlCommand sc1 = new SqlCommand("INSERT into DellDefenseDB values(@fileName,@fileContent)",con1);
                            sc1.Parameters.AddWithValue("@fileName", f);
                            sc1.Parameters.AddWithValue("@fileContent", content1);
                            sc1.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error in writing to database" + ex);
                        }
                    }
                }
            }
            finally
            {
                con1.Close();
                MessageBox.Show("Data loaded to database");
            }                       
        }
    }
}
