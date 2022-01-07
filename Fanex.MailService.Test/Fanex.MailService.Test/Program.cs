using Fanex.MailDeliver.MailClient;
using System;

namespace Fanex.MailService.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Fanex.MailService.Test!");

            AppSettings.AppMailId = "1";
            AppSettings.AppMailAddress = "http://localhost:31599/";

            try
            {
                var testMailService = new TestCase();
                testMailService.Test3_SendMail();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }
}