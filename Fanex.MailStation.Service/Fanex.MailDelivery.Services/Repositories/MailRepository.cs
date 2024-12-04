using Fanex.Data;
using Fanex.MailDeliver.MailClient.Model;
using Fanex.MailDelivery.Core.Models;
using Fanex.MailDelivery.Services.Criteria;
using Fanex.MailDelivery.Services.Services;
using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace Fanex.MailDelivery.Services.Repositories;

public interface IMailRepository
{
    Schedule GetScheduleByMailId(GetScheduleByMailIdCriteria criteria);

    int AddMailTask(AddMailTaskCriteria criteria);

    void UpdateMailTaskStatus(UpdateMailTaskStatusCriteria criteria);

    void UpdateRetryCounter(UpdateTaskRetryCounterCriteria criteria);

    void AddMailLog(AddMailLogCriteria criteria);

    MailingList GetMailingList(GetMailWithEmailAddressByMailId criteria);

    void UpdateMailTask(UpdateMailTaskMSiteMailIdCriteria criteria);

    void UpdateClientActiveTime(UpdateClientActiveTimeCriteria criteria);

    MailTaskModel GetMailTaskById(GetMailTaskByIdCriteria criteria);

    MailRecord GetMailRecordById(int mailId);

    void UpdateServiceCheckIn(ServiceMonitoringCheckInCriteria criteria);

    void UpdateFailedForLongRunningMailTask();

    long InsertMailRecord(MailRecord mailRecord);

    MailRecord GetMailRecord(long mailID, bool isSend);
}

public class MailRepository : IMailRepository
{
    public Schedule GetScheduleByMailId(GetScheduleByMailIdCriteria criteria)
    {
        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
        return db.QueryEntity<Schedule>(criteria);
    }

    public void UpdateClientActiveTime(UpdateClientActiveTimeCriteria criteria)
    {
        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
        db.ExecuteNonQuery(criteria);
    }

    public void UpdateServiceCheckIn(ServiceMonitoringCheckInCriteria criteria)
    {
        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
        db.ExecuteNonQuery(criteria);
    }

    public void UpdateFailedForLongRunningMailTask()
    {
        var criteria = new UpdateFailedForLongRunningMailTaskCriteria();

        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
        db.ExecuteNonQuery(criteria);
    }

    public int AddMailTask(AddMailTaskCriteria criteria)
    {
        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
        db.ExecuteNonQuery(criteria);
        return Convert.ToInt32(db.GetParameterValue("MailTaskID"));
    }

    public MailTaskModel GetMailTaskById(GetMailTaskByIdCriteria criteria)
    {
        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
        return db.QueryEntity<MailTaskModel>(criteria);
    }

    public void UpdateMailTaskStatus(UpdateMailTaskStatusCriteria criteria)
    {
        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
        db.ExecuteNonQuery(criteria);
    }

    public void UpdateRetryCounter(UpdateTaskRetryCounterCriteria criteria)
    {
        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
        db.ExecuteNonQuery(criteria);
    }

    public void AddMailLog(AddMailLogCriteria criteria)
    {
        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
        db.ExecuteNonQuery(criteria);
    }

    public MailingList GetMailingList(GetMailWithEmailAddressByMailId criteria)
    {
        var result = new MailingList();
        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());

        var reader = db.ExecuteReader(criteria);
        if (reader.Read())
        {
            result.Id = Convert.ToInt32(reader["MailId"]);
            result.Name = Convert.ToString(reader["MailName"]);
            result.SiteId = Convert.ToInt16(reader["SiteId"]);
            result.Active = Convert.ToBoolean(reader["IsActive"]);
            result.Group = Convert.ToInt16(reader["MailGroupID"]);
            result.TOs = Convert.ToString(reader["MailTo"])!.Split(';').ToList();
            result.CCs = Convert.ToString(reader["MailCc"])!.Split(';').ToList();
            result.BCCs = Convert.ToString(reader["MailBcc"])!.Split(';').ToList();
        }

        return result;
    }

    public void UpdateMailTask(UpdateMailTaskMSiteMailIdCriteria criteria)
    {
        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
        db.ExecuteNonQuery(criteria);
    }

    public MailRecord GetMailRecordById(int mailId)
    {
        var criteria = new MailRecorderGetMailById { MailId = mailId };
        var mailRecord = new MailRecord();
        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());

        var dataSet = db.ExecuteDataSet(criteria);
        var dataTable = new DataTable();
        if (dataSet != null && dataSet.Tables.Count > 0)
        {
            dataTable = dataSet.Tables[0];
        }

        foreach (DataRow row in dataTable.Rows)
        {
            mailRecord.SentSuccess = Convert.ToInt32(row["SentSuccess"]);
            mailRecord.SentFail = Convert.ToInt32(row["SentFail"]);
        }

        return mailRecord;
    }

    public long InsertMailRecord(MailRecord mailRecord)
    {
        if (mailRecord.Config != null && mailRecord.Config.AppID <= 0 && mailRecord.AppID <= 0)
        {
            return 0;
        }

        var currentAppId = mailRecord.Config != null ? mailRecord.Config.AppID : mailRecord.AppID;
        var currentApp = GetMailConfig(currentAppId);
        if (currentApp != null && currentApp.IsEncrypt)
        {
            mailRecord.IsEncrypt = currentApp.IsEncrypt;
        }

        return InsertMailRecord2(mailRecord);
    }

    private MailConfig GetMailConfig(int appID)
    {
        if (appID <= 0)
        {
            return null;
        }

        Hashtable param = new Hashtable();
        param.Add("appid", appID);
        using var objDb = ObjectDbFactory.CreateInstance("GetConfig");

        var resultDataset = objDb.ExecuteDataSet(param);
        if (resultDataset.Tables.Count > 0
            && resultDataset.Tables[0].Rows.Count > 0)
        {
            return GetMailConfigFromDataRow(resultDataset.Tables[0].Rows[0]);
        }

        return new MailConfig();
    }

    private MailConfig GetMailConfigFromDataRow(DataRow mailConfigDataRow)
    {
        return new MailConfig
        {
            AppID = Convert.ToInt32(mailConfigDataRow["appid"]),
            AppName = Convert.ToString(mailConfigDataRow["appname"]),
            Description = Convert.ToString(mailConfigDataRow["description"]),
            SettingID = Convert.ToInt32(mailConfigDataRow["settingid"]),
            MailFrom = Convert.ToString(mailConfigDataRow["mailfrom"]),
            IsEncrypt = Convert.ToBoolean(mailConfigDataRow["isencrypt"]),
            Setting = new MailSetting
            {
                SettingID = Convert.ToInt32(mailConfigDataRow["settingid"]),
                SettingName = Convert.ToString(mailConfigDataRow["settingname"]),
                Username = Convert.ToString(mailConfigDataRow["username"]),
                Password = Convert.ToString(mailConfigDataRow["password"]),
                IsAuth = Convert.ToBoolean(mailConfigDataRow["isauth"]),
                Server = Convert.ToString(mailConfigDataRow["server"]),
                Port = Convert.ToInt32(mailConfigDataRow["port"])
            }
        };
    }

    private long InsertMailRecord2(MailRecord mailItem)
    {
        long mailID;

        var param = new Hashtable();
        param.Add("mailid", mailItem.MailID);
        param.Add("appid", mailItem.Config != null ? mailItem.Config.AppID : mailItem.AppID);
        param.Add("funcid", mailItem.Function != null ? mailItem.Function.FuncID : mailItem.FuncID);
        param.Add("mailto", mailItem.IsEncrypt ? Cryptography.EncryptPassword(mailItem.MailTo ?? string.Empty) : mailItem.MailTo);
        param.Add("mailcc", mailItem.IsEncrypt ? Cryptography.EncryptPassword(mailItem.MailCc ?? string.Empty) : mailItem.MailCc);
        param.Add("mailbcc", mailItem.IsEncrypt ? Cryptography.EncryptPassword(mailItem.MailBcc ?? string.Empty) : mailItem.MailBcc);
        param.Add("title", mailItem.Title);
        param.Add("subject", mailItem.Subject);
        param.Add("body", mailItem.IsEncrypt ? Cryptography.EncryptPassword(mailItem.Body ?? string.Empty) : mailItem.Body);
        param.Add("maildate", DateTime.Now);
        param.Add("attachments", string.Empty);
        param.Add("isencrypt", mailItem.IsEncrypt);

        // store attachment in local file
        if (mailItem.Attachments != null)
        {
            var fileNames = mailItem.Attachments.Select(s => s.FileName).ToArray();
            param["attachments"] = string.Join(",", fileNames);
        }

        using var objDb = ObjectDbFactory.CreateInstance("StoreMail");

        objDb.ExecuteNonQuery(param);
        mailID = Math.Max(Convert.ToInt64(param["mailid"]), Convert.ToInt64(param["MAILID"]));
        if (mailID > 0)
        {
            if (mailItem.Attachments != null)
            {
                new MailAttachmentManager().StoreMailAttachments(mailID, mailItem.Attachments);
            }

            if (mailItem.AlternateView != null)
            {
                new LinkedResourcesManager().Insert(mailID, mailItem.AlternateView);
            }
        }

        return mailID;
    }

    public MailRecord GetMailRecord(
        long mailID,
        bool isSend = false)
    {
        var param = new Hashtable();
        param.Add("mailid", mailID);
        using var objDb = ObjectDbFactory.CreateInstance("GetMail");

        var result = objDb.ExecuteDataSet(param);
        if (result.Tables.Count > 0)
        {
            var mailRecordRow = result.Tables[0].Rows[0];
            var mailItem = GetMailRecordFromDataRow(mailRecordRow, true);
            if (isSend)
            {
                mailItem.IsReady = true;

                // get attachment file
                var attachments = new MailAttachmentManager().GetMailAttachments(mailID);
                mailItem.Attachments = attachments == null ? null : attachments.ToList();

                // get linked resource
                var resources = new LinkedResourcesManager().GetLinkedResources(mailID);
                mailItem.AlternateView = resources == null ? null : resources.ToList();
            }

            return mailItem;
        }

        return null;
    }

    private MailRecord GetMailRecordFromDataRow(DataRow mailRecordDataRow, bool isGetMailBody = false)
    {
        var funcId = mailRecordDataRow["funcid"] == DBNull.Value ? 0 : Convert.ToInt32(mailRecordDataRow["funcid"]);
        return new MailRecord
        {
            AppID = Convert.ToInt32(mailRecordDataRow["appid"]),
            Config = new MailConfig
            {
                AppID = Convert.ToInt32(mailRecordDataRow["appid"]),
                AppName = Convert.ToString(mailRecordDataRow["appname"]),
                Setting = new MailSetting
                {
                    SettingID = Convert.ToInt32(mailRecordDataRow["settingid"]),
                    SettingName = Convert.ToString(mailRecordDataRow["settingname"]),
                    Username = Convert.ToString(mailRecordDataRow["username"]),
                    Password = Convert.ToString(mailRecordDataRow["password"]),
                    IsAuth = Convert.ToBoolean(mailRecordDataRow["isauth"]),
                    Server = Convert.ToString(mailRecordDataRow["server"]),
                    Port = Convert.ToInt32(mailRecordDataRow["port"])
                }
            },
            FuncID = funcId,
            Function = new MailFunction
            {
                FuncID = funcId,
                FuncName = Convert.ToString(mailRecordDataRow["funcname"])
            },
            MailID = Convert.ToInt64(mailRecordDataRow["mailid"]),
            Title = Convert.ToString(mailRecordDataRow["title"]),
            Subject = Convert.ToString(mailRecordDataRow["subject"]),
            MailTo = Convert.ToString(mailRecordDataRow["mailto"]),
            MailCc = Convert.ToString(mailRecordDataRow["mailcc"]),
            MailBcc = Convert.ToString(mailRecordDataRow["mailbcc"]),
            MailDate = Convert.ToString(mailRecordDataRow["maildate"]),
            MailUpdatedDate = mailRecordDataRow["mailupdateddate"] == DBNull.Value ? Convert.ToString(mailRecordDataRow["mailupdateddate"]) : DateTime.Now.ToString(),
            Body = isGetMailBody ? Convert.ToString(mailRecordDataRow["body"]) : string.Empty,
            FileAttachments = Convert.ToString(mailRecordDataRow["attachments"]),
            CountSend = Convert.ToInt32(mailRecordDataRow["CountSend"]),
            SentSuccess = mailRecordDataRow["SentSuccess"] != DBNull.Value ? Convert.ToInt32(mailRecordDataRow["SentSuccess"]) : 0,
            SentFail = mailRecordDataRow["SentFail"] != DBNull.Value ? Convert.ToInt32(mailRecordDataRow["SentFail"]) : 1,
            IsEncrypt = Convert.ToBoolean(mailRecordDataRow["IsEncrypt"])
        };
    }
}