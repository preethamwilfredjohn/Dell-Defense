using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using log4net;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace DellDefense
{
    class Job : IJob
    {
        SqlConnection compareConnection  = new SqlConnection();
        Hashing hash = new Hashing();
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public async Task Execute(IJobExecutionContext context)
        {
            //initiaizing logger
            log4net.Config.XmlConfigurator.Configure();
            ComparingFiles compareFiles = new ComparingFiles();
            //intializing job
            JobKey key = context.JobDetail.Key;

            //retriving data from scheduler
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string dataSource = dataMap.GetString("dataSource");
            string dataBase = dataMap.GetString("dataBase");
            string userName = dataMap.GetString("userName");
            string password = dataMap.GetString("password");
            string dirPath = dataMap.GetString("dirPath");
            string email = dataMap.GetString("email");

            //initializing variables and methods for email
            string from = "Dell Defense Tool";            
            string subjectFailure = "Dell Defense Status - Failed";                        
            MailMessage mailobj = new MailMessage();            
            mailobj.From = new MailAddress("delldefense@gmail.com", from);
            mailobj.To.Add(new MailAddress(email));            
            mailobj.IsBodyHtml = true;
            SmtpClient SMTPClient = new SmtpClient();
            SMTPClient.Host = "smtp.gmail.com";
            SMTPClient.Port = 587;
            SMTPClient.Credentials = new NetworkCredential("delldefense@gmail.com", "");
            SMTPClient.EnableSsl = true;            
            log.Info("A new cycle has been started by Dell Defense");
            string connectionString = "Data Source=" + dataSource + ";Initial Catalog=" + dataBase + ";User ID=" + userName + ";Password=" + password;
            compareConnection = new SqlConnection(connectionString);
            //connecting with database
            try
            {
                compareConnection.Open();
                compareFiles.CompareDB(dirPath, compareConnection, email);                
            }
            catch (SqlException ex)
            {
                log.Error("Cannot connect to database with exception " + ex);
                //sending email
                mailobj.Body = "Cannot connect to database with exception " + ex;
                mailobj.Subject = subjectFailure;
                try
                {
                    SMTPClient.Send(mailobj);
                }
                catch (SmtpException e)
                {
                    log.Error("Error in sending email with exception" + e);
                }
            }
            //closing db connection
            finally
            {
                log.Info("Closing DataBase connection");
                compareConnection.Close();
            }
            log.Info("Validated all the files. Completed cycle.");
            mailobj.Body = "Validation cycle completed. Please check logs for more information.";
            mailobj.Subject = "Job Cycle Completed.";
            try
            {
                SMTPClient.Send(mailobj);
            }
            catch (SmtpException e)
            {
                log.Error("Error in sending email with exception" + e);
            }
        }
    }
}
