using Fanex.MailDeliver.MailClient;
using Fanex.MailDeliver.MailClient.Model;
using System;
using System.IO;

namespace Fanex.MailService_FW
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            AppSettings.AppMailId = "1";
            AppSettings.AppMailAddress = "http://10.18.200.131:8787/";
            //AppSettings.AppMailAddress = "http://localhost:31599/";

            var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var mail = new MailRecord
            {
                MailTo = "robert.tran@starixsoft.com",
                Title = "Robert Test at Local",
                Subject = $"Robert Test NEW lib, run on App FW at Local - {DateTime.Now}",
                Body = "Robert Test at Local",
                Attachments = new System.Collections.Generic.List<MailAttachment>
                {
                    new MailAttachment
                    {
                        FileName = @"Template/Book1.xlsx"
                    }
                }
            };

            mailService.SendMail(mail);
        }
    }
}