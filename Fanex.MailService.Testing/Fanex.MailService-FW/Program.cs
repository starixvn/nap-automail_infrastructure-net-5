using Fanex.MailDeliver.MailClient;
using Fanex.MailDeliver.Model;
using Fanex.MailDeliver.Utility;
using System;

namespace Fanex.MailService_FW
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var person = new Person();
            person.Name = "Robert";

            var encript = MailFormatter.Serialize(person);
            var decript = MailFormatter.Deserialize<Person>(encript);

            AppSettings.AppMailId = "1";
            AppSettings.AppMailAddress = "http://10.18.200.131:8787/";

            var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var mail = new MailRecord
            {
                MailTo = "robert.tran@starixvn.com",
                Title = "Robert Test at Local",
                Subject = $"Robert Test NEW lib, run on App FW at Local - {DateTime.Now}",
                Body = "Robert Test at Local",
            };

            mailService.SendMail(mail);
        }
    }
}