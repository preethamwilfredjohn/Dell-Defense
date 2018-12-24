using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using log4net;
using System.Net.Mail;
using System.Net;

namespace DellDefenseCore
{
    class Schedule
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public async void Start(DateTime startDateTime, DateTime endDateTime, string dataSource, string dataBase, string userName, string password, string dirPath, string email, int jobInterval)
        {
            String from = "Dell Defense Tool";
            String subjectFailure = "Dell Defense Status - Failed";
            //declaring variables and methods related to email
            MailMessage mailobj = new MailMessage();
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
                ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
                IScheduler scheduler = await schedulerFactory.GetScheduler();

                //starting the scheduler
                await scheduler.Start();

                log.Info("Creating Job.");
                //job defenition and mapping with the task in the mainWindow
                IJobDetail job = JobBuilder.Create<Job>()
                   .UsingJobData("dataSource", dataSource)
                   .UsingJobData("dataBase", dataBase)
                   .UsingJobData("userName", userName)
                   .UsingJobData("password", password)
                   .UsingJobData("dirPath", dirPath)
                   .UsingJobData("email", email)
                    .Build();

                log.Info("Creating trigger for the Job.");
                //creating a trigger to run the job
                ITrigger trigger = TriggerBuilder.Create()
                   .WithIdentity("DellDefense", "group1")
                   .StartAt(startDateTime)
                   .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(jobInterval)
                        .RepeatForever())
                   .WithPriority(1)
                   .EndAt(endDateTime)
                   .Build();

                log.Info("Firing the trigger for the Job.");
                //schedulingthe job using the trigger
                await scheduler.ScheduleJob(job, trigger);

            }
            catch (SchedulerException ex)
            {
                log.Error("Job failed");

                //sending email
                mailobj.Body = "Dell Defense tool unable to create job with an exception - " + ex;
                mailobj.Subject = subjectFailure;
                try
                {
                    SMTPClient.Send(mailobj);
                }
                catch (Exception e)
                {
                    log.Error("Error in sending email with exception" + e);
                }
            }
        }
    }
}
