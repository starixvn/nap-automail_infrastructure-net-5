using Fanex.MailDeliver.MailClient;
using System;
using System.IO;

namespace Fanex.MailService.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine("Fanex.MailService.Test!");

            AppSettings.AppMailId = "1";
            //AppSettings.AppMailAddress = "http://localhost:31599/";
            AppSettings.AppMailAddress = "http://10.18.200.131:8787/";

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