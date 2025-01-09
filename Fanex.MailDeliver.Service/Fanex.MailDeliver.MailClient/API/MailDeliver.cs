using Fanex.MailDeliver.MailClient.Model;

namespace Fanex.MailDeliver.MailClient.API;

using WCF;
using ParamModel;
using Utility;
using System;
using System.Collections.Generic;

public class MailDeliver : IMailDeliver, IDisposable
{
    private readonly MailServiceClient mailClient;
    private readonly MailRecord mailRecord;

    public MailDeliver()
    {
        // Get mail address service
        if (!string.IsNullOrEmpty(AppSettings.AppMailAddress))
        {
            mailClient = new MailServiceClient();
            mailRecord = new MailRecord();
        }
        else
        {
            throw new Exception("The appsettings key \"app_mail_address\" which didn't support");
        }
    }

    public void SendMail(
        MailRecord mail,
        bool canRetry = true)
    {
        if (mail != null)
        {
            if (!string.IsNullOrEmpty(AppSettings.AppMailId)
                || (mail.Config != null && mail.Config.AppID > 0))
            {
                if (mail.Config == null || mail.Config.AppID == 0)
                {
                    var appid = Convert.ToInt32(AppSettings.AppMailId);
                    appid = mail.AppID > 0 ? mail.AppID : appid;
                    mail.Config = new MailConfig
                    {
                        AppID = appid
                    };
                }

                Util.AttachmentUtil.Format(ref mail);

                mail.CanRetry = canRetry;

                mail.Subject = FilterSpecialCharacters(mail.Subject);
                mail.Title = FilterSpecialCharacters(mail.Title);
                mail.MailTo = FilterSpecialCharacterForEmail(mail.MailTo);
                mail.MailCc = FilterSpecialCharacterForEmail(mail.MailCc);
                mail.MailBcc = FilterSpecialCharacterForEmail(mail.MailBcc);

                mailClient.SendMail(mail);
            }
            else
            {
                throw new Exception("The appsettings key \"AppMailId\" which didn't support ");
            }
        }
    }

    private string FilterSpecialCharacters(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var filteredText = text
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\t", string.Empty)
            .Replace("&nbsp;", " ");

        var specialCharactersConfig = AppSettings.SpecialCharacters;
        if (!string.IsNullOrWhiteSpace(specialCharactersConfig))
        {
            var specialCharacters = specialCharactersConfig.Split(',');

            foreach (var specialCharacter in specialCharacters)
            {
                filteredText = filteredText.Replace(specialCharacter, string.Empty);
            }
        }

        return filteredText;
    }

    private string FilterSpecialCharacterForEmail(string emailRaw)
    {
        if (string.IsNullOrWhiteSpace(emailRaw))
        {
            return emailRaw;
        }

        return FilterSpecialCharacters(
            emailRaw
                .Replace("  ", string.Empty)
                .Replace(" ", string.Empty));
    }

    public MailRecord GetMail(long mailId)
    {
        MailRecord record = null;
        if (mailId > 0)
        {
            var buffer = mailClient.GetMail(mailId);
            if (buffer != null)
            {
                record = MailFormatter.Deserialize<MailRecord>(buffer);
            }
        }

        return record;
    }

    public IEnumerable<MailRecord> GetListMail(
        int appId,
        DateTime mailDate,
        int page,
        int counter)
    {
        var mailList = new List<MailRecord>();
        if (appId > 0)
        {
            var param = new GetListMailParam { AppId = appId, MailDate = mailDate, Page = page, Counter = counter };
            var buffers = mailClient.GetListMail(param);
            if (buffers != null)
            {
                foreach (var buffer in buffers)
                {
                    var record = MailFormatter.Deserialize<MailRecord>(buffer);
                    if (record != null)
                    {
                        mailList.Add(record);
                    }
                }
            }
        }

        return mailList;
    }

    public IEnumerable<MailRecord> GetListMailByDate(
        int appId,
        DateTime fromDate,
        DateTime toDate,
        ref int totalRows,
        int page,
        int counter,
        MailStatus status)
    {
        var mailList = new List<MailRecord>();
        if (appId >= 0)
        {
            var param = new GetListMailByDateParam
            {
                AppId = appId,
                FromDate = fromDate,
                ToDate = toDate,
                TotalRows = totalRows,
                Page = page,
                Counter = counter,
                Status = (int)status
            };
            var result = mailClient.GetListMailByDate(param);
            var buffers = result.Item1;
            totalRows = result.Item2;
            if (buffers != null)
            {
                foreach (var buffer in buffers)
                {
                    var record = MailFormatter.Deserialize<MailRecord>(buffer);
                    if (record != null)
                    {
                        mailList.Add(record);
                    }
                }
            }
        }

        return mailList;
    }

    public MailConfig StoreConfig(MailConfig config)
    {
        if (config != null)
        {
            return MailFormatter.Deserialize<MailConfig>(mailClient.StoreConfig(config));
        }

        return null;
    }

    public MailConfig GetConfig(int appId)
    {
        MailConfig config = null;

        if (appId > 0)
        {
            var buffer = mailClient.GetConfig(appId);
            if (buffer != null)
            {
                config = MailFormatter.Deserialize<MailConfig>(buffer);
            }
        }

        return config;
    }

    #region Mail Setting

    public MailSetting StoreSetting(MailSetting mailSetting)
    {
        if (mailSetting != null)
        {
            return MailFormatter.Deserialize<MailSetting>(mailClient.StoreSetting(mailSetting));
        }

        return null;
    }

    public MailSetting GetSetting(int settingID)
    {
        MailSetting setting = null;

        if (settingID > 0)
        {
            var buffer = mailClient.GetSetting(settingID);
            if (buffer != null)
            {
                setting = MailFormatter.Deserialize<MailSetting>(buffer);
            }
        }

        return setting;
    }

    #endregion Mail Setting

    #region resend mail

    public void ReSendMail(long mailID)
    {
        if (mailID > 0)
        {
            mailClient.ReSendMail(mailID);
        }
    }

    #endregion resend mail

    #region mail function

    public MailFunction StoreFunction(MailFunction func)
    {
        return func != null ? MailFormatter.Deserialize<MailFunction>(mailClient.StoreFunction(func)) : null;
    }

    public MailFunction GetFunction(int funcID)
    {
        MailFunction func = null;

        if (funcID > 0)
        {
            var buffer = mailClient.GetFunction(funcID);
            if (buffer != null)
            {
                func = MailFormatter.Deserialize<MailFunction>(buffer);
            }
        }

        return func;
    }

    #endregion mail function

    #region get list setting

    public IEnumerable<MailSetting> GetListSetting()
    {
        var settings = new List<MailSetting>();

        var buffers = mailClient.GetListSetting();
        if (buffers != null)
        {
            foreach (var buffer in buffers)
            {
                var setting = MailFormatter.Deserialize<MailSetting>(buffer);
                if (setting != null)
                {
                    settings.Add(setting);
                }
            }
        }

        return settings;
    }

    public IEnumerable<MailConfig> GetListConfig(string serverIp)
    {
        var configs = new List<MailConfig>();
        var buffers = mailClient.GetListConfig(serverIp);
        if (buffers != null)
        {
            foreach (var buffer in buffers)
            {
                var config = MailFormatter.Deserialize<MailConfig>(buffer);
                if (config != null)
                {
                    configs.Add(config);
                }
            }
        }

        return configs;
    }

    public IEnumerable<MailFunction> GetListFuntion()
    {
        var functions = new List<MailFunction>();
        var buffers = mailClient.GetListFuntion();
        if (buffers != null)
        {
            foreach (var buffer in buffers)
            {
                var func = MailFormatter.Deserialize<MailFunction>(buffer);
                if (func != null)
                {
                    functions.Add(func);
                }
            }
        }

        return functions;
    }

    #endregion get list setting

    public IEnumerable<MailError> GetListError(long mailID)
    {
        var errors = new List<MailError>();
        var buffers = mailClient.GetListError(mailID);
        if (buffers != null)
        {
            foreach (var buffer in buffers)
            {
                var error = MailFormatter.Deserialize<MailError>(buffer);
                if (error != null)
                {
                    errors.Add(error);
                }
            }
        }

        return errors;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Fluent Interface
    public IMailDeliver CreateMail(string mailTitle = "")
    {
        mailRecord.Title = mailTitle;
        return this;
    }

    public IMailDeliver AddSubject(string mailSubject = "")
    {
        mailRecord.Subject = mailSubject;
        return this;
    }

    public IMailDeliver To(string toMail = "")
    {
        mailRecord.MailTo = toMail;
        return this;
    }

    public IMailDeliver AddSendMailApplication(int mailApplication = 0)
    {
        mailRecord.Config.AppID = mailApplication;
        return this;
    }

    public IMailDeliver AddSendMailFunction(int mailFunction = 0)
    {
        mailRecord.Function.FuncID = mailFunction;
        return this;
    }

    public IMailDeliver AddBody(string mailBody = "")
    {
        mailRecord.Body = mailBody;
        return this;
    }

    public IMailDeliver ToCcMails(string toCCMails = "")
    {
        mailRecord.MailCc = toCCMails;
        return this;
    }

    public IMailDeliver ToBccMails(string toBccMails = "")
    {
        mailRecord.MailBcc = toBccMails;
        return this;
    }

    public IMailDeliver AddTitle(string mailTitle = "")
    {
        mailRecord.Title = mailTitle;
        return this;
    }

    public IMailDeliver AddAttachments(List<MailAttachment> mailAttachments)
    {
        if (mailAttachments != null)
        {
            mailRecord.Attachments = mailAttachments;
        }

        return this;
    }

    public IMailDeliver AddAlternativeViews(List<MailLinkedResource> mailAlternativeViews)
    {
        if (mailAlternativeViews != null)
        {
            mailRecord.AlternateView = mailAlternativeViews;
        }

        return this;
    }

    public IMailDeliver SendMail(bool canRetry = true)
    {
        mailRecord.CanRetry = canRetry;
        SendMail(mailRecord, mailRecord.CanRetry);
        return this;
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}