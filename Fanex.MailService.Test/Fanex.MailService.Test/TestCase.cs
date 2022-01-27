using Fanex.MailDeliver.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fanex.MailService.Test
{
    public class TestCase
    {
        public void Test3_SendMail()
        {
            var fileName = "Template/xuan.jpg";
            var stream = File.Open(fileName, FileMode.Open);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();

            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var mail = new MailRecord
            {
                MailTo = "robert.tran@starixvn.com",
                Title = "Robert Test at Local",
                Subject = $"Robert Test at Local - {DateTime.Now}",
                Body = "Robert Test at Local",
                //Attachments = new System.Collections.Generic.List<MailAttachment>
                // {
                //    new MailAttachment
                //    {
                //        FileName = "Template/Book1.xlsx"
                //    },
                //    new MailAttachment
                //    {
                //        FileName = "Template/xuan.jpg",
                //        ContentType = FileExtentions.IMG_JPG
                //    },
                // }

                Attachments = new System.Collections.Generic.List<MailAttachment>
                 {
                    new MailAttachment(new MemoryStream(buffer), FileExtentions.IMG_JPG){ FileName= "xuan.jpg"}
                 }
            };

            mailService.SendMail(mail);
        }

        public void Test4_GetMail()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var mailId = 1796370;
            var mailRecord = mailService.GetMail(mailId);
        }

        public void Test5_GetListMail()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var mails = mailService.GetListMail(1, DateTime.Today.AddDays(-1), 1, 1);
        }

        public void Test6_GetListMailByDate()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var totalRows = 0;
            var mails = mailService.GetListMailByDate(1, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1), ref totalRows, 1, 1, MailStatus.All);
            var json = JsonConvert.SerializeObject(mails, Formatting.Indented);
            Console.WriteLine(json);
        }

        public void Test7_StoreConfig()
        {
            var mailConfig = new MailConfig
            {
                AppID = 61,
                AppName = "ZAP1",
                Description = "ZAP1",
                IsEncrypt = false,
                MailFrom = "ZAP@abc.com",
            };

            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var cfg = mailService.StoreConfig(mailConfig);
        }

        public void Test8_GetConfig()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var result = mailService.GetConfig(1);
        }

        public void Test9_StoreSetting()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var mailSettings = new MailSetting
            {
                Username = "R",
                Password = "1234",
                IsAuth = false,
                Port = 1234,
                Server = "10.0.0.1",
                SettingID = 100,
                SettingName = "Test"
            };
            var result = mailService.StoreSetting(mailSettings);
        }

        public void Test10_GetSetting()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var result = mailService.GetSetting(1);
        }

        public void Test11_ReSendMail()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            mailService.ReSendMail(1807205);
        }

        public void Test12_StoreFunction()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var mailFunc = new MailFunction
            {
                AppID = 1,
                FuncID = 14,
                FuncName = "R Test",
                SettingID = 1,
            };
            var rs = mailService.StoreFunction(mailFunc);
        }

        public void Test13_GetFunction()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var rs = mailService.GetFunction(14);
        }

        public void Test14_GetListSetting()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var rs = mailService.GetListSetting();
        }

        public void Test15_GetListConfig()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var rs = mailService.GetListConfig("");
        }

        public void Test16_GetListFuntion()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var rs = mailService.GetListFuntion();
        }

        public void Test17_GetListError()
        {
            using var mailService = new MailDeliver.MailClient.API.MailDeliver();
            var rs = mailService.GetListError(0);
        }
    }
}