namespace Fanex.MailDeliver.MailClient.API.WCF
{
    using Fanex.MailDeliver.MailClient.ParamModel;
    using Fanex.MailDeliver.MailClient.Util;
    using Fanex.MailDeliver.Model;
    using System;
    using System.Collections.Generic;

    public class MailServiceClient : IMailService
    {
        public void SendMail(MailRecord mailRecord)
        {
            var action = $"MailService/SendMail";
            WebApiClientServiceImp.PostAsync<bool, MailRecord>(action, mailRecord);
        }

        public byte[] GetMail(long mailId)
        {
            var action = $"MailService/GetMail";
            return WebApiClientServiceImp.PostAsync<byte[], long>(action, mailId);
        }

        public IEnumerable<byte[]> GetListMail(GetListMailParam param)
        {
            var action = $"MailService/GetListMail";
            return WebApiClientServiceImp.PostAsync<IEnumerable<byte[]>, GetListMailParam>(action, param);
        }

        public Tuple<IEnumerable<byte[]>, int> GetListMailByDate(GetListMailByDateParam param)
        {
            var action = $"MailService/GetListMailByDate";
            return WebApiClientServiceImp.PostAsync<Tuple<IEnumerable<byte[]>, int>, GetListMailByDateParam>(action, param);
        }

        public byte[] StoreConfig(MailConfig mailConfig)
        {
            var action = $"MailService/StoreConfig";
            return WebApiClientServiceImp.PostAsync<byte[], MailConfig>(action, mailConfig);
        }

        public byte[] GetConfig(int appId)
        {
            var action = $"MailService/GetConfig";
            return WebApiClientServiceImp.PostAsync<byte[], int>(action, appId);
        }

        public byte[] StoreSetting(MailSetting mailSetting)
        {
            var action = $"MailService/StoreSetting";
            return WebApiClientServiceImp.PostAsync<byte[], MailSetting>(action, mailSetting);
        }

        public byte[] GetSetting(int settingID)
        {
            var action = $"MailService/GetSetting";
            return WebApiClientServiceImp.PostAsync<byte[], int>(action, settingID);
        }

        public bool ReSendMail(long mailID)
        {
            var action = $"MailService/ReSendMail";
            return WebApiClientServiceImp.PostAsync<bool, long>(action, mailID);
        }

        public byte[] StoreFunction(MailFunction mailFunction)
        {
            var action = $"MailService/StoreFunction";
            return WebApiClientServiceImp.PostAsync<byte[], MailFunction>(action, mailFunction);
        }

        public byte[] GetFunction(int funcID)
        {
            var action = $"MailService/GetFunction";
            return WebApiClientServiceImp.PostAsync<byte[], int>(action, funcID);
        }

        public IEnumerable<byte[]> GetListSetting()
        {
            var action = $"MailService/GetListSetting";
            return WebApiClientServiceImp.PostAsync<IEnumerable<byte[]>, int>(action, 0);
        }

        public IEnumerable<byte[]> GetListConfig(string serverIp)
        {
            var action = $"MailService/GetListConfig";
            return WebApiClientServiceImp.PostAsync<IEnumerable<byte[]>, string>(action, serverIp);
        }

        public IEnumerable<byte[]> GetListFuntion()
        {
            var action = $"MailService/GetListFuntion";
            return WebApiClientServiceImp.PostAsync<IEnumerable<byte[]>, int>(action, 0);
        }

        public IEnumerable<byte[]> GetListError(long mailID)
        {
            var action = $"MailService/GetListError";
            return WebApiClientServiceImp.PostAsync<IEnumerable<byte[]>, long>(action, mailID);
        }
    }
}