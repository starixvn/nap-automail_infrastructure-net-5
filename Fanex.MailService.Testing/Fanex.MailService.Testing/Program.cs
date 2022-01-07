using Fanex.MailDeliver.Model;
using System;

namespace Fanex.MailService.Testing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var mailService = new MailDeliver.MailClient.API.MailDeliver();
                var mail = new MailRecord
                {
                    MailTo = "robert.tran@starixvn.com",
                    Title = "Robert Test at Local",
                    Subject = $"Robert Test OLD lib at Local - {DateTime.Now}",
                    Body = "Robert Test at Local",
                };

                mailService.SendMail(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadLine();
        }
    }
}