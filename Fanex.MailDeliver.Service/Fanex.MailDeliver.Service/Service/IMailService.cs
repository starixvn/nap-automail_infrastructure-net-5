using Fanex.MailDeliver.MailClient.Model;
using Fanex.MailDeliver.MailClient.ParamModel;
using System;
using System.Collections.Generic;

namespace Fanex.MailDeliver.Service.Service
{
    public interface IMailService
    {
        void SendMail(MailRecord mailRecord);

        byte[] StoreConfig(MailConfig mailConfig);

        byte[] GetMail(long mailID);

        IEnumerable<byte[]> GetListMail(GetListMailParam param);

        Tuple<IEnumerable<byte[]>, int> GetListMailByDate(GetListMailByDateParam param);/*int appID, DateTime fromDate, DateTime toDate, ref int totalRows, int page, int counter, int status*/

        byte[] GetConfig(int appID);

        byte[] StoreSetting(MailSetting mailSetting);

        byte[] GetSetting(int settingID);

        void ReSendMail(long mailID);

        byte[] StoreFunction(MailFunction mailFunction);

        byte[] GetFunction(int funcID);

        IEnumerable<byte[]> GetListSetting();

        IEnumerable<byte[]> GetListConfig(string serverIp);

        IEnumerable<byte[]> GetListFuntion();

        IEnumerable<byte[]> GetListError(long mailID);
    }
}