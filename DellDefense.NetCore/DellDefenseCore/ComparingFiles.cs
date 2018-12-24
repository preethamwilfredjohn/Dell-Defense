using log4net;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace DellDefenseCore
{
    class ComparingFiles
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        string from = "Dell Defense Tool";
        string subjectFailure = "Dell Defense Status - Failed";
        MailMessage mailobj = new MailMessage();
        Hashing hash = new Hashing();
        public void CompareDB(string path, SqlConnection compareConnection, string email)
        {
            mailobj.From = new MailAddress("delldefense@gmail.com", from);
            mailobj.To.Add(new MailAddress(email));
            mailobj.IsBodyHtml = true;
            SmtpClient SMTPClient = new SmtpClient();
            SMTPClient.Host = "smtp.gmail.com";
            SMTPClient.Port = 587;
            SMTPClient.Credentials = new NetworkCredential("delldefense@gmail.com", "capfa18g3");
            SMTPClient.EnableSsl = true;
            try
            {
                foreach (string file in Directory.GetFiles(path))
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
                foreach (string directory in Directory.GetDirectories(path))
                {
                    CompareDB(directory, compareConnection, email);
                }
            }
            catch (SqlException ex)
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
            catch (IOException ex)
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
    }
}
