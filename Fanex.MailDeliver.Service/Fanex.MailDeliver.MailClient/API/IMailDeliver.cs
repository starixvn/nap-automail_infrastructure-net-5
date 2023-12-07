using Fanex.MailDeliver.MailClient.Model;

namespace Fanex.MailDeliver.MailClient.API
{
    using System;
    using System.Collections.Generic;

    public interface IMailDeliver
    {
        void SendMail(MailRecord mail, bool canRetry = true);

        MailRecord GetMail(long mailId);

        IEnumerable<MailRecord> GetListMail(int appId, DateTime mailDate, int page, int counter);

        IEnumerable<MailRecord> GetListMailByDate(int appId, DateTime fromDate, DateTime toDate, ref int totalRows, int page, int counter, MailStatus status);

        MailConfig StoreConfig(MailConfig config);

        MailConfig GetConfig(int appId);

        MailSetting StoreSetting(MailSetting mailSetting);

        MailSetting GetSetting(int settingID);

        void ReSendMail(long mailID);

        MailFunction StoreFunction(MailFunction func);

        MailFunction GetFunction(int funcID);

        IEnumerable<MailSetting> GetListSetting();

        IEnumerable<MailConfig> GetListConfig(string serverIp);

        IEnumerable<MailFunction> GetListFuntion();

        // Fluent interface
        IMailDeliver CreateMail(string mailTitle = "");

        IMailDeliver AddSubject(string mailSubject = "");

        IMailDeliver To(string toMail = "");

        IMailDeliver AddSendMailApplication(int mailApplication = 0);

        IMailDeliver AddSendMailFunction(int mailFunction = 0);

        IMailDeliver AddBody(string mailBody = "");

        IMailDeliver AddTitle(string mailTitle = "");

        IMailDeliver ToCcMails(string toCCMails = "");

        IMailDeliver ToBccMails(string toBccMails = "");

        IMailDeliver AddAttachments(List<MailAttachment> mailAttachments);

        IMailDeliver AddAlternativeViews(List<MailLinkedResource> mailAlternativeViews);

        IMailDeliver SendMail(bool canRetry = true);

        IEnumerable<MailError> GetListError(long mailID);
    }
}