using System;
using log4net;
using System.Data.SqlClient;
using System.IO;

namespace DellDefenseCore
{
    class Details
    {
       private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);        
        Hashing hash = new Hashing();
        public string maskPassword()
        {
            string passwordCW = null;
            do
            {
                ConsoleKeyInfo pwkey = Console.ReadKey(true);
                // Backspace Should Not Work
                if (pwkey.Key != ConsoleKey.Backspace && pwkey.Key != ConsoleKey.Enter)
                {
                    passwordCW += pwkey.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (pwkey.Key == ConsoleKey.Backspace && passwordCW.Length > 0)
                    {
                        passwordCW = passwordCW.Substring(0, (passwordCW.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (pwkey.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);
            Console.WriteLine("");
            return passwordCW;
        }
        public string ChoosePath()
        {
            bool temp = false;
            string location = null;
            do
            {
                if(temp == false)
                {
                    Console.WriteLine("Please enter directory location: ");
                    location = Console.ReadLine();
                    temp = true;
                }
                else
                {
                    Console.WriteLine("Please enter appropriate directory location: ");
                    location = Console.ReadLine();
                }
               
            } while (string.IsNullOrEmpty(location));
            log.Info("File directory location entered.");
            return location;
        }
        public bool CheckDBConnection(string dataSource, string dbName,string userName, string password)
        {
            bool temp = false;
            string connectionString = "Data Source=" + dataSource + ";Initial Catalog=" + dbName + ";User ID=" + userName + ";Password=" + password;
            log.Info("Checking database connection");
            SqlConnection con = new SqlConnection(connectionString);
            try
            {
                log.Info("Trying to connect to DataBase");
                con.Open();
                log.Info("Connected to DataBase");
                Console.WriteLine("Connection Sccessful");
                temp = true;
                log.Info("Closing DataBase connection");
                con.Close();                
            }
            catch (SqlException ex)
            {
                log.Error("Connection to Database failed with exception " + ex);
                Console.WriteLine("Connection not established. Please validate your input. Check logs for more details.");
            }
            return temp;
        } 
        public void LoadDatabase(string dataSource, string dbName, string userName, string password,string filePath)
        {
            log.Info("---------------------Loading Database Phase---------------------");
            string connectionString = "Data Source=" + dataSource + ";Initial Catalog=" + dbName + ";User ID=" + userName + ";Password=" + password;
            SqlConnection con1 = new SqlConnection(connectionString);
            try
            {
                log.Info("Establishing connection to database");
                con1.Open();
                log.Info("Removing pre exsisting data from DataBase");
                SqlCommand del = new SqlCommand("delete from DellDefenseDB", con1);
                del.ExecuteNonQuery();
                //hashing and loading the root directory files
                FindingFiles.loadDB(filePath, con1);                
                log.Info("Data loaded to database");
                Console.WriteLine("Data loaded into Database");
            }
            catch (SqlException ex)
            {
                log.Error("Connection to Database failed with exception " + ex);
                Console.WriteLine("Connection to Database failed with exception " + ex);
            }
            //closing database connection
            finally
            {
                con1.Close();
                log.Info("DataBase Connection closed to database");
            }
        }
        public void startComparing(DateTime startDateTime, DateTime endDateTime, string dataSource, string dataBase, string userName, string password, string dirPath, string email, int jobInterval)
        {                             
            Schedule startApplication = new Schedule();
            log.Info("Starting the application");                
            string checkD = null;
            string connectionString = "Data Source=" + dataSource + ";Initial Catalog=" + dataBase + ";User ID=" + userName + ";Password=" + password;
            SqlConnection compareConnection = new SqlConnection(connectionString);
            try
            {
                log.Info("Trying to connect to DataBase");
                compareConnection.Open();     
                //checking if the database has content
                SqlCommand checkData = new SqlCommand("Select top 1 fileName from DellDefenseDB", compareConnection);
                SqlDataReader readFirst = checkData.ExecuteReader();
                while (readFirst.Read())
                {
                    checkD = (string) readFirst["fileName"];
                }
                readFirst.Close();
                checkData.Dispose();
                if (string.IsNullOrEmpty(checkD))
                {
                    Console.WriteLine("No rows in DataBase. Please load data into Database");
                }
                else
                {                
                    Console.WriteLine("Application is running and will be completed by the end time specified");
                    startApplication.Start(startDateTime, endDateTime, dataSource, dataBase, userName, password, dirPath, email, jobInterval);
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
    }
}
