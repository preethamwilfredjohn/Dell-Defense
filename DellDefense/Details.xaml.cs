﻿using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Data.SqlClient;
using System;
using log4net;

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
        public MainWindow()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
        }

        private void PathButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Choose Path button clicked");
            dialog.InitialDirectory = @"C:\Users\preet\Desktop\Test";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePathTextBox.Text = dialog.FileName;
            }            
        }

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
                catch (Exception ex)
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

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Start Button Clicked");
            if (!string.IsNullOrWhiteSpace(FilePathTextBox.Text) && !string.IsNullOrWhiteSpace(DataSourceTextBox.Text) && !string.IsNullOrWhiteSpace(DBNameTextBox.Text) && !string.IsNullOrWhiteSpace(DBPasswordBox.Password) && !string.IsNullOrWhiteSpace(UserIDTextBox.Text))
            {
                string checkD = null;
                string connectionString = "Data Source=" + DataSourceTextBox.Text + ";Initial Catalog=" + DBNameTextBox.Text + ";User ID=" + UserIDTextBox.Text + ";Password=" + DBPasswordBox.Password;
                compareConnection = new SqlConnection(connectionString);
                try
                {
                    log.Info("Trying to connect to DataBase");
                    compareConnection.Open();
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
                                        log.Info("Reading data from DataBase");
                                        while (readDB.Read())
                                        {
                                            fileContentDB = (string)readDB["fileContent"];
                                        }
                                    }
                                }
                                log.Info("Comparing data from database with local file - " + file);
                                if (content != fileContentDB)
                                {
                                    log.Fatal("Validataion not successfull.Please check the file at " + file);
                                    MessageBox.Show("Validataion not successfull.Please check the file at " + file);
                                    break;
                                }
                                else
                                {
                                    log.Info("Validation Successful for the file - " + file);
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error reading file from database " + ex);
                                MessageBox.Show("error reading file from database" + ex);
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
                                        using (SqlDataReader readDB1 = sc1.ExecuteReader())
                                        {
                                            log.Info("Reading data from DataBase");
                                            while (readDB1.Read())
                                            {
                                                fileContentDB1 = (string)readDB1["fileContent"];
                                            }
                                        }
                                    }
                                    log.Info("Comparing data from database with local file - " + f);
                                    if (content1 != fileContentDB1)
                                    {
                                        log.Fatal("Validataion not successfull.Please check the file at " + f);
                                        MessageBox.Show("Validataion not successfull.Please check the file at " + f);
                                        break;
                                    }
                                    else
                                    {
                                        log.Info("Validation Successful for the file - " + f);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Error reading from Database with exception " + ex);
                                }
                            }
                        }
                    }                                        
                    
                    log.Info("Validated all the files. No files were modified in this cycle.");
                    MessageBox.Show("Completed validation");
                }
                catch(Exception ex)
                {
                    log.Error("Connection to Database failed with exception " + ex);                   
                }
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

        private void Button_Click(object sender, RoutedEventArgs e)
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
                    SqlCommand del = new SqlCommand("delete from DellDefenseDB",con1);
                    del.ExecuteNonQuery();
                    foreach (string file in Directory.EnumerateFiles(dialog.FileName))
                    {
                        try
                        {
                            string content = File.ReadAllText(file);
                            SqlCommand sc = new SqlCommand("INSERT into DellDefenseDB values(@fileName,@fileContent)", con1);
                            sc.Parameters.AddWithValue("@fileName", file);
                            sc.Parameters.AddWithValue("@fileContent", content);
                            log.Info("Inserting file details {0} into database" + file);
                            sc.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error while writing to database" + ex);
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
                                SqlCommand sc1 = new SqlCommand("INSERT into DellDefenseDB values(@fileName,@fileContent)", con1);
                                sc1.Parameters.AddWithValue("@fileName", f);
                                sc1.Parameters.AddWithValue("@fileContent", content1);
                                log.Info("Inserting file details {0} into database" + f);
                                sc1.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error while writing to database" + ex);
                            }
                        }
                    }
                    log.Info("Data loaded to database");
                }
                catch (Exception ex)
                {
                    log.Error("Connection to Database failed with exception " + ex);
                }
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
    }
}
