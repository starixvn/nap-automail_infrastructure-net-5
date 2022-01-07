using Fanex.MailDeliver.MailClient;
using Fanex.MailDeliver.Model;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Fanex.MailService.NET5.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello World! - Testing Mail Service");
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                var text = File.ReadAllText("Template/Config.json");
                var config = JsonConvert.DeserializeObject<Config>(text);

                AppSettings.AppMailId = config.AppMailId;
                AppSettings.AppMailAddress = config.AppMailAddress;

                var mailService = new MailDeliver.MailClient.API.MailDeliver();
                var mail = new MailRecord
                {
                    MailTo = "robert.tran@starixvn.com",
                    Title = "Robert Test at Local",
                    Subject = $"Robert Test NEW lib, run on App NET5 at Local - {DateTime.Now}",
                    Body = "Robert Test at Local",
                };

                mailService.SendMail(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Sent Mail OK");
            Console.ReadLine();
        }
    }
}