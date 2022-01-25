using Fanex.MailDeliver.MailClient.API;
using Fanex.MailDeliver.Model;
using System;

namespace FW.MailService.Old
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                using (var mailService = new MailDeliver())
                {
                    var mail = new MailRecord
                    {
                        MailTo = "robert.tran@starixvn.com",
                        Title = "Robert Test at Local",
                        Subject = $"Robert Test at Local - {DateTime.Now}",
                        Body = "Robert Test at Local",
                        Attachments = new System.Collections.Generic.List<MailAttachment>
                 {
                    new MailAttachment
                    {
                        FileName = "Template/Book1.xlsx"
                    },
                    new MailAttachment
                    {
                        FileName = "Template/xuan.jpg",
                        ContentType = FileExtentions.IMG_JPG
                    },
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