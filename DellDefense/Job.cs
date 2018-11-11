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
            SMTPClient.Credentials = new NetworkCredential("delldefense@gmail.com", "capfa18g3");
            SMTPClient.EnableSsl = true;            
            log.Info("A new cycle has been started by Dell Defense");
            string connectionString = "Data Source=" + dataSource + ";Initial Catalog=" + dataBase + ";User ID=" + userName + ";Password=" + password;
            compareConnection = new SqlConnection(connectionString);
            //connecting with database
            try
            {
                compareConnection.Open();
                try
                {
                    //comparing data with local file
                    //iterating through root directory
                    foreach (string file in Directory.GetFiles(dirPath))
                    {
                        string hashedContent = Hashing.BytesToString(hash.GetHashSha256(file));
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
                            if (hashedContent != fileContentDB)
                            {
                                log.Fatal("Validataion not successfull. Please check the file at " + file);
                                //sending email
                                mailobj.Body = "Validataion not successfull. Please check the file at " + file;
                                mailobj.Subject = subjectFailure;
                                try
                                {
                                   SMTPClient.Send(mailobj);
                                }
                                catch (SmtpException ex)
                                {
                                    log.Error("Error in sending email with exception" + ex);
                                }                                
                            }
                            else
                            {
                                log.Info("Validation Successful for the file - " + file);
                            }
                        }
                        catch (SqlException ex)
                        {
                            log.Error("Error reading file from database with exception - " + ex);
                            mailobj.Body = "Error reading file from database with exception - " + ex;
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
                    }
                    string dir = dirPath;
                    //comparing files in sub directories
                    foreach (string d in Directory.GetDirectories(dir))
                    {
                        foreach (string f in Directory.GetFiles(d))
                        {
                            string fileContentDB1 = null;
                            string hashedContent = Hashing.BytesToString(hash.GetHashSha256(f));                            
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
                                if (hashedContent != fileContentDB1)
                                {
                                    log.Fatal("Validataion not successfull.Please check the file at " + f);
                                    //sending email
                                    mailobj.Body = "Validataion not successfull. Please check the file at " + f;
                                    mailobj.Subject = subjectFailure;
                                    try
                                    {
                                        SMTPClient.Send(mailobj);
                                    }
                                    catch (SmtpException ex)
                                    {
                                        log.Error("Error in sending email with exception" + ex);
                                    }
                                }
                                else
                                {
                                    log.Info("Validation Successful for the file - " + f);
                                }
                            }
                            catch (SqlException ex)
                            {
                                log.Error("Error reading from Database with exception " + ex);
                                //sending email
                                mailobj.Body = "Error reading from Database with exception - " + ex;
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
                        }
                    }
                }
                catch(SqlException ex)
                {
                    log.Error("Error while reading from database with exception " + ex);
                    //sending email
                    mailobj.Body = "Error reading from Database with exception - " + ex;
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
                catch(IOException ex)
                {
                    log.Error("Error while reading from file with Exception -" + ex);
                    //sending email
                    mailobj.Body = "Error while reading from file with Exception -" + ex;
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
        }
    }
}
