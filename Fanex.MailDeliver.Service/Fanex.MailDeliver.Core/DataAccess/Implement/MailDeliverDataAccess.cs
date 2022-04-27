// --------------------------------------------------------------------------
// <copyright file="MailDeliverDataAccess.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
//   <change who="Ricky.Nguyen" date="2012.9.24">Update get MailFunction</change>
//   <change who="Ricky.Nguyen" date="2012.9.24">Update MailUpdatedDate property of MailRecord</change>
// </history>

namespace Fanex.MailDeliver.DataAccess.Implement
{
    using Fanex.Data;
    using Fanex.MailDeliver.Core.Criterias;
    using Fanex.MailDeliver.DataAccess.Interface;
    using Fanex.MailDeliver.Model;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public class MailDeliverDataAccess : IMailDeliverDataAccess
    {
        public long InsertMailRecord(MailRecord mailItem)
        {
            long mailID = 0;

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

            using (ObjectDb objDb = new ObjectDb("StoreMail"))
            {
                objDb.ExecuteNonQuery(param);
                mailID = Math.Max(Convert.ToInt64(param["mailid"]), Convert.ToInt64(param["MAILID"]));
                if (mailID > 0)
                {
                    if (mailItem.Attachments != null)
                    {
                        (new MailAttachmentManager()).StoreMailAttachments(mailID, mailItem.Attachments);
                    }

                    if (mailItem.AlternateView != null)
                    {
                        (new LinkedResourcesManager()).Insert(mailID, mailItem.AlternateView);
                    }
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
            using (ObjectDb objDb = new ObjectDb("GetMail"))
            {
                var result = objDb.ExecuteDataSet(param);
                if (result.Tables.Count > 0)
                {
                    var mailRecordRow = result.Tables[0].Rows[0];
                    var mailItem = GetMailRecordFromDataRow(mailRecordRow, true);
                    if (isSend)
                    {
                        mailItem.IsReady = true;

                        // get attachment file
                        var attachments = (new MailAttachmentManager()).GetMailAttachments(mailID, mailItem.FileAttachments);
                        mailItem.Attachments = attachments == null ? null : attachments.ToList();

                        // get linked resource
                        var resources = (new LinkedResourcesManager()).GetLinkedResources(mailID);
                        mailItem.AlternateView = resources == null ? null : resources.ToList();
                    }

                    return mailItem;
                }
                else
                {
                    return null;
                }
            }
        }

        public MailConfig SaveMailConfig(MailConfig config)
        {
            var param = new Hashtable();
            param.Add("appid", config.AppID);
            param.Add("appname", config.AppName);
            param.Add("description", config.Description);
            param.Add("settingid", config.Setting != null ? config.Setting.SettingID : config.SettingID);
            param.Add("mailfrom", config.MailFrom ?? string.Empty);
            param.Add("isencrypt", config.IsEncrypt);
            param.Add("userid", config.UserId);
            using (ObjectDb objDb = new ObjectDb("StoreConfig"))
            {
                objDb.ExecuteNonQuery(param);
                config.AppID = Math.Max(Convert.ToInt32(param["appid"]), Convert.ToInt32(param["AppID"]));
            }

            return config;
        }

        public IEnumerable<MailRecord> GetListMailRecordByDate(
            int appID,
            DateTime mailDate,
            int page = 1,
            int counter = 100)
        {
            List<MailRecord> records = new List<MailRecord>();
            var param = new Hashtable();
            param.Add("appid", appID);
            param.Add("maildate", mailDate);
            param.Add("page", page);
            param.Add("counter", counter);
            using (ObjectDb objDb = new ObjectDb("GetListMail"))
            {
                var result = objDb.ExecuteDataSet(param);
                var rows = result.Tables[0].Rows;
                foreach (DataRow rowMail in rows)
                {
                    var mailItem = new MailRecord
                    {
                        AppID = Convert.ToInt32(rowMail["appid"]),
                        //FuncID = Convert.ToInt32(rowMail["funcid"]),
                        MailID = Convert.ToInt64(rowMail["mailid"]),
                        Title = Convert.ToString(rowMail["title"]),
                        Subject = Convert.ToString(rowMail["subject"]),
                        MailTo = Convert.ToString(rowMail["mailto"]),
                        MailCc = Convert.ToString(rowMail["mailcc"]),
                        MailBcc = Convert.ToString(rowMail["mailbcc"]),
                        MailDate = Convert.ToString(rowMail["maildate"]),
                        Body = string.Empty,
                        FileAttachments = Convert.ToString(rowMail["attachments"]),
                        //CountSend = Convert.ToInt32(rowMail["CountSend"]),
                        //SentSuccess = Convert.ToInt32(rowMail["SentSuccess"]),
                        //SentFail = Convert.ToInt32(rowMail["SentFail"])
                    };
                    records.Add(mailItem);
                }
            }

            return records;
        }

        /// <summary>
        /// Get List Mail from date to date.
        /// </summary>
        /// <param name="appID">Application ID.</param>
        /// <param name="fromDate">From Date query.</param>
        /// <param name="toDate">To Date query.</param>
        /// <param name="totalRows">Total rows.</param>
        /// <param name="page">Page Number.</param>
        /// <param name="counter">Record per page.</param>
        /// <param name="status">Mail status.</param>
        /// <returns>List Mail Record.</returns>
        public IEnumerable<MailRecord> GetListMailRecordByDateRange(
            int appID,
            DateTime fromDate,
            DateTime toDate,
            ref int totalRows,
            int page = 1,
            int counter = 100,
            int status = 0)
        {
            List<MailRecord> records = new List<MailRecord>();
            var param = new Hashtable();
            param.Add("appid", appID);
            param.Add("fromdate", fromDate);
            param.Add("todate", toDate);
            param.Add("page", page);
            param.Add("counter", counter);
            param.Add("status", status);
            param.Add("totalrows", 0);
            using (ObjectDb objDb = new ObjectDb("GetListMailByDate"))
            {
                var result = objDb.ExecuteDataSet(param);
                var rows = result.Tables[0].Rows;
                foreach (DataRow mailRecordRow in rows)
                {
                    try
                    {
                        records.Add(GetMailRecordFromDataRow(mailRecordRow));
                    }
                    catch (Exception ex)
                    {
                        var s = ex.Message;
                    }
                }
            }

            totalRows = Convert.ToInt32(param["TOTALROWS"]);

            return records;
        }

        /// <summary>
        /// Get Config for send mail.
        /// </summary>
        /// <param name="appID">Application Id.</param>
        /// <returns>Mail Config.</returns>
        public MailConfig GetMailConfig(int appID)
        {
            if (appID <= 0)
            {
                return null;
            }

            Hashtable param = new Hashtable();
            param.Add("appid", appID);
            using (ObjectDb objDb = new ObjectDb("GetConfig"))
            {
                var resultDataset = objDb.ExecuteDataSet(param);
                if (resultDataset.Tables.Count > 0
                    && resultDataset.Tables[0].Rows.Count > 0)
                {
                    return GetMailConfigFromDataRow(resultDataset.Tables[0].Rows[0]);
                }

                return new MailConfig();
            }
        }

        /// <summary>
        /// Save Mail Setting.
        /// </summary>
        /// <param name="setting">Mail setting information.</param>
        /// <returns>Mail setting after save success.</returns>
        public MailSetting SaveMailSetting(MailSetting setting)
        {
            var param = new Hashtable();
            param.Add("settingid", setting.SettingID);
            param.Add("settingname", setting.SettingName);
            param.Add("isauth", setting.IsAuth);
            param.Add("username", setting.Username);
            param.Add("password", setting.Password);
            param.Add("server", setting.Server);
            param.Add("port", setting.Port);
            using (ObjectDb objDb = new ObjectDb("StoreSetting"))
            {
                objDb.ExecuteNonQuery(param);
                setting.SettingID = Convert.ToInt32(param["settingid"]);
            }

            return setting;
        }

        /// <summary>
        /// Get Setting from Setting ID.
        /// </summary>
        /// <param name="settingID">Setting ID.</param>
        /// <returns>Mail Setting.</returns>
        public MailSetting GetMailSetting(int settingID)
        {
            using (ObjectDb objDb = new ObjectDb("GetSetting"))
            {
                return objDb.QueryEntity<MailSetting>(new { SettingID = settingID });
            }
        }

        /// <summary>
        /// Save setting.
        /// </summary>
        /// <param name="func">Mail function.</param>
        /// <returns>Mail function data.</returns>
        public MailFunction SaveMailFunction(MailFunction func)
        {
            var param = new Hashtable();
            param.Add("funcid", func.FuncID);
            param.Add("appid", func.Config != null ? func.Config.AppID : func.AppID);
            param.Add("funcname", func.FuncName);
            param.Add("settingid", func.Setting != null ? func.Setting.SettingID : func.SettingID);
            using (ObjectDb objDb = new ObjectDb("StoreFunction"))
            {
                objDb.ExecuteNonQuery(param);
                var result = objDb.GetOuputParams();
                if (Convert.ToInt32(result["errcode"]) == 0)
                {
                    func.FuncID = Convert.ToInt32(result["funcid"]);
                    return func;
                }
                else
                {
                    throw new Exception(result["errmsg"].ToString());
                }
            }
        }

        /// <summary>
        /// Get Function from Function ID.
        /// </summary>
        /// <param name="funcID">Function ID.</param>
        /// <returns>Mail Function.</returns>
        public MailFunction GetMailFunction(int funcID)
        {
            MailFunction mailFunction = new MailFunction();
            var inputParams = new Hashtable();
            inputParams.Add("funcid", funcID);
            using (ObjectDb objDb = new ObjectDb("GetFunction"))
            {
                var resultDataset = objDb.ExecuteDataSet(inputParams);
                if (resultDataset.Tables.Count > 0
                   && resultDataset.Tables[0].Rows.Count > 0)
                {
                    var functionRow = resultDataset.Tables[0].Rows[0];
                    mailFunction = GetMailFunctionFromDataRow(functionRow);
                }

                return mailFunction;
            }
        }

        /// <summary>
        /// Update Mail status after send mail.
        /// </summary>
        /// <param name="mailID">Mail ID need to update.</param>
        /// <param name="status">Send mail status.</param>
        public void UpdateMailStatus(
            long mailID,
            bool status)
        {
            if (mailID > 0)
            {
                var param = new Hashtable();
                param.Add("mailid", mailID);
                param.Add("status", status);
                param.Add("mailupdateddate", DateTime.Now);
                using (ObjectDb objDb = new ObjectDb("UpdateMailStatus"))
                {
                    objDb.ExecuteNonQuery(param);
                }
            }
        }

        /// <summary>
        /// Get List Setting for send mail.
        /// </summary>
        /// <returns>Return List Mail Setting.</returns>
        public List<MailSetting> GetListMailSetting()
        {
            using (ObjectDb objDb = new ObjectDb("GetListSetting"))
            {
                return objDb.Query<MailSetting>().ToList();
            }
        }

        /// <summary>
        /// Get List Config for Send Mail From DB.
        /// </summary>
        /// <param name="serverIp">Server address.</param>
        /// <returns>List Mail Config.</returns>
        public List<MailConfig> GetListMailConfig(string serverIp = "")
        {
            List<MailConfig> mailConfigs = new List<MailConfig>();
            using (ObjectDb objDb = new ObjectDb("GetListConfig"))
            {
                var resultDataset = objDb.ExecuteDataSet(new { Ip = serverIp });
                if (resultDataset.Tables.Count > 0
                    && resultDataset.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow configRow in resultDataset.Tables[0].Rows)
                    {
                        mailConfigs.Add(GetMailConfigFromDataRow(configRow));
                    }
                }

                return mailConfigs;
            }
        }

        /// <summary>
        /// Get List Function from DB.
        /// </summary>
        /// <returns>Return list of Function.</returns>
        public List<MailFunction> GetListMailFuntion()
        {
            List<MailFunction> mailFunctions = new List<MailFunction>();
            using (ObjectDb objDb = new ObjectDb("GetListFunction"))
            {
                var resultDataset = objDb.ExecuteDataSet();
                if (resultDataset.Tables.Count > 0
                    && resultDataset.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow functionRow in resultDataset.Tables[0].Rows)
                    {
                        mailFunctions.Add(GetMailFunctionFromDataRow(functionRow));
                    }
                }

                return mailFunctions;
            }
        }

        /// <summary>
        /// Insert mail error when send mail was error.
        /// </summary>
        /// <param name="criteria">Mail error information.</param>
        /// <returns>Inserted error id.</returns>
        public long InsertMailError(InsertMailErrorCriteria criteria)
        {
            long errorID = 0;

            using (ObjectDb objDb = new ObjectDb(criteria.GetSettingKey()))
            {
                var parameters = criteria.GetSpParams();
                objDb.ExecuteNonQuery(parameters);
                errorID = Convert.ToInt64(parameters["errorid"]);
            }

            return errorID;
        }

        /// <summary>
        /// Get Mail Function from DataRow.
        /// </summary>
        /// <param name="mailFunctionDataRow">Mail Function Data row.</param>
        /// <returns>Mail Function.</returns>
        private MailFunction GetMailFunctionFromDataRow(DataRow mailFunctionDataRow)
        {
            return new MailFunction
            {
                FuncID = Convert.ToInt32(mailFunctionDataRow["funcid"].ToString()),
                FuncName = Convert.ToString(mailFunctionDataRow["funcname"].ToString()),
                AppID = Convert.ToInt32(mailFunctionDataRow["appid"].ToString()),
                Config = new MailConfig
                {
                    AppID = Convert.ToInt32(mailFunctionDataRow["appid"].ToString()),
                    AppName = Convert.ToString(mailFunctionDataRow["appname"].ToString()),
                    Description = Convert.ToString(mailFunctionDataRow["description"].ToString()),
                    Setting = new MailSetting
                    {
                        SettingID = Convert.ToInt32(mailFunctionDataRow["appsettingid"].ToString()),
                        SettingName = Convert.ToString(mailFunctionDataRow["appsettingname"].ToString()),
                        Server = Convert.ToString(mailFunctionDataRow["appsettingserver"].ToString()),
                        Port = Convert.ToInt32(mailFunctionDataRow["appsettingport"].ToString())
                    }
                },
                SettingID = Convert.ToInt32(mailFunctionDataRow["settingid"].ToString()),
                Setting = new MailSetting
                {
                    SettingID = Convert.ToInt32(mailFunctionDataRow["settingid"].ToString()),
                    SettingName = Convert.ToString(mailFunctionDataRow["settingname"].ToString()),
                    Server = Convert.ToString(mailFunctionDataRow["server"].ToString()),
                    Port = Convert.ToInt32(mailFunctionDataRow["port"].ToString()),
                    IsAuth = Convert.ToBoolean(mailFunctionDataRow["isauth"].ToString()),
                    Password = Convert.ToString(mailFunctionDataRow["password"].ToString()),
                    Username = Convert.ToString(mailFunctionDataRow["username"].ToString())
                }
            };
        }

        /// <summary>
        /// Get Mail Record from Data row after query from Database.
        /// </summary>
        /// <param name="mailRecordDataRow">Mail Record row.</param>
        /// <param name="isGetMailBody">Is Get Mail body or not.</param>
        /// <returns>Mail Record.</returns>
        private MailRecord GetMailRecordFromDataRow(DataRow mailRecordDataRow, bool isGetMailBody = false)
        {
            var funcid = mailRecordDataRow["funcid"] == DBNull.Value ? 0 : Convert.ToInt32(mailRecordDataRow["funcid"]);
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
                FuncID = funcid,
                Function = new MailFunction
                {
                    FuncID = funcid,
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

        /// <summary>
        /// Get Mail Config from Data row.
        /// </summary>
        /// <param name="mailConfigDataRow">Mail config data row.</param>
        /// <returns>Mail config data.</returns>
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
    }
}