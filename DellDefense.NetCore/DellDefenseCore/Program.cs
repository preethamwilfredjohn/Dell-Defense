using System;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net;
using log4net.Config;

namespace DellDefenseCore
{
    class Program
    {        
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        public static string userName = "preethamwilfredjohn@gmail.com";
        private static string Password = "DellDefense";
        static void Main(string[] args)
        {            
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            var logger = LogManager.GetLogger(typeof(Program));

            //--------------for testing uncoment these lines
            //string dataSource = "dcm.uhcl.edu";
            //string dataBase = "capfa18g3";
            //string userNameDB = "capfa18g3";
            //string passwordDB = "3163345";

            string directoryPath = null;
            string dataSource = null;
            string dataBase = null;
            string userNameDB = null;
            string passwordDB = null;
            int jobInterval = 0;
            DateTime stTime;
            DateTime eTime;
            bool loggedin = false;
            bool dbCheck = false;
            bool stTimeCheck = true;
            bool eTimeCheck = true;
            bool jobIntervalCheck = true;

            do
            {
                Details det = new Details();
                Console.WriteLine("Enter user Name: ");
                string userNameCW = Console.ReadLine();
                Console.WriteLine("Enter Password: ");
                string passwordCW = det.maskPassword();
                log.Info("Validating User Name and Password");                

                //compare user name and password
                if (userNameCW == userName)
                {
                    if (passwordCW == Password)
                    {
                        loggedin = true;
                        log.Info("User Name and password matches.");
                        Console.WriteLine("Log-In Successfull");
                        //asking the user to input the path                    
                        directoryPath = det.ChoosePath();
                        log.Info("Path to monitor has been set to - " + directoryPath);
                        do
                        {
                            Console.WriteLine("Enter Data Source: ");
                            dataSource = Console.ReadLine();
                            Console.WriteLine("Enter DataBase name: ");
                            dataBase = Console.ReadLine();
                            Console.WriteLine("Enter DataBase user name: ");
                            userNameDB = Console.ReadLine();
                            Console.WriteLine("Enter Database password: ");
                            passwordDB = det.maskPassword();
                            if (!string.IsNullOrEmpty(dataSource) && !string.IsNullOrEmpty(dataBase) && !string.IsNullOrEmpty(userNameDB) && !string.IsNullOrEmpty(passwordDB))
                            {
                                dbCheck = det.CheckDBConnection(dataSource, dataBase, userNameDB, passwordDB);
                            }
                            else
                            {
                                Console.WriteLine("--------------------Please enter appropriate DataBase details--------------------");
                            }
                        } while (dbCheck != true);

                        //getting the start time for the application
                        do
                        {
                            if (stTimeCheck == true)
                            {
                                Console.WriteLine("Enter start date and time for the application to start in the format (MM/DD/YYYY hh:mm:ss): ");
                                var startTime = Console.ReadLine();
                                stTimeCheck = DateTime.TryParse(startTime, out stTime);
                            }
                            else
                            {
                                Console.WriteLine("Enter appropriate start date and time for the application to start in the format (MM/DD/YYYY hh:mm:ss):");
                                var startTime = Console.ReadLine();
                                stTimeCheck = DateTime.TryParse(startTime, out stTime);
                            }

                        } while (stTimeCheck != true);
                        log.Info("Start time for the application set to " + stTime);
                        //getting the end time for the application
                        do
                        {
                            if (eTimeCheck == true)
                            {
                                Console.WriteLine("Enter end date and time for the application to end in the format (MM/DD/YYYY hh:mm:ss): ");
                                var endTime = Console.ReadLine();
                                eTimeCheck = DateTime.TryParse(endTime, out eTime);
                            }
                            else
                            {
                                Console.WriteLine("Enter appropriate end date and time for the application to end in the format (MM/DD/YYYY hh:mm:ss): ");
                                var endTime = Console.ReadLine();
                                eTimeCheck = DateTime.TryParse(endTime, out eTime);
                            }

                        } while (eTimeCheck != true);
                        log.Info("End time for the application set to " + eTime);

                        //getting the job interval time
                        do
                        {
                            if (jobIntervalCheck==true)
                            {
                                Console.WriteLine("Enter job interval in minutes: ");
                                string jobIntString = Console.ReadLine();
                                jobIntervalCheck = !string.IsNullOrEmpty(jobIntString);
                                if (!string.IsNullOrEmpty(jobIntString))
                                {
                                    jobInterval = Convert.ToInt32(jobIntString);
                                }
                            }
                            else
                            {
                                Console.WriteLine("--------------------Please enter appropriate job interval--------------------");
                                jobIntervalCheck = true;
                            }
                        } while (jobInterval==0);
                        
                        log.Info("Job interval set to " + jobInterval + " minutes.");

                        //loading the database with the file content
                        det.LoadDatabase(dataSource, dataBase, userNameDB, passwordDB, directoryPath);
                        //starting to compare the files
                        det.startComparing(stTime, eTime, dataSource, dataBase, userNameDB, passwordDB, directoryPath, userName, jobInterval);
                        Console.WriteLine("Waiting for the application to complete the cycle.");
                        Console.ReadKey();
                        Console.ReadKey();
                    }
                }
                else
                {
                    log.Info("Invalid username and password");
                    Console.WriteLine("");
                    Console.WriteLine("Incorrect username and password");
                    loggedin = false;
                }
            }while(!loggedin);            
        }
    }
}
