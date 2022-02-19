using Fanex.MailDeliver.MailClient;
using Fanex.MailDeliver.MailClient.API;
using Fanex.MailDeliver.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace AppNetFW
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AppSettings.AppMailId = "34";
            //AppSettings.AppMailAddress = "http://10.18.200.131:8787/";
            AppSettings.AppMailAddress = "http://localhost:31599/";

            try
            {
                var fileName = "Template/xuan.jpg";
                var stream = File.Open(fileName, FileMode.Open);
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Close();

                using (var mailService = new MailDeliver())
                {
                    var mail = new MailRecord
                    {
                        MailTo = "robert.tran@starixvn.com",
                        Title = "Robert Test at Local",
                        Subject = $"Robert Test at Local - {DateTime.Now}",
                        Body = "Robert Test at Local",
                        //Attachments = new List<MailAttachment>
                        //{
                        //    //new MailAttachment
                        //    //{
                        //    //    FileName = "Template/Book1.xlsx"
                        //    //},
                        //    //new MailAttachment
                        //    //{
                        //    //    FileName = "Template/xuan.jpg",
                        //    //    ContentType = FileExtentions.IMG_JPG
                        //    //},
                        //}

                        Attachments = new List<MailAttachment>
                         {
                            new MailAttachment(new MemoryStream(buffer), FileExtentions.IMG_JPG){ FileName= "xuan.jpg"}
                         }
                    };

                    mailService.SendMail(mail);
                    Console.WriteLine("Send Mail successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send Mail ERR {ex.Message}.");
            }

            Console.ReadLine();
        }
    }
}