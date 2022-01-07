namespace Fanex.MailDeliver.Business.Interface
{
    using Fanex.MailDeliver.Model;
    using System;
    using System.Collections.Generic;

    public interface IMailDeliverBusiness
    {
        byte[] SaveMailConfig(MailConfig mailConfig);

        byte[] GetMailRecordByStream(long mailID);

        MailRecord GetMailRecord(long mailID, bool isSend = false);

        IEnumerable<byte[]> GetListMailRecordByDate(int appID, DateTime mailDate, int page, int counter);

        IEnumerable<byte[]> GetListMailRecordByDateRange(
            int appID,
            DateTime fromDate,
            DateTime toDate,
            ref int totalRows,
            int page,
            int counter,
            int status);

        byte[] GetMailConfig(int appID);

        byte[] SaveMailSetting(MailSetting mailSetting);

        byte[] GetMailSetting(int settingID);

        byte[] SaveMailFunction(MailFunction mailFunction);

        byte[] GetMailFunction(int funcID);

        IEnumerable<byte[]> GetListMailSetting();

        IEnumerable<byte[]> GetListMailConfig(string serverIp = "");

        IEnumerable<byte[]> GetListMailFuntion();

        void SendMail(MailRecord record);

        IEnumerable<byte[]> GetListMailError(long mailID);

        byte[] SaveMailError(byte[] buffer);
    }
}