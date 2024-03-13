using Fanex.Data;
using Fanex.Data.Repository;
using Fanex.MailDeliver.Core.Business.Interface;
using Fanex.MailDeliver.Core.Business.TableCaching;
using Fanex.MailDeliver.Core.Criterias;
using Fanex.MailDeliver.Core.DataAccess.Interface;
using Fanex.MailDeliver.Core.Infrastructure.Logging;
using Fanex.MailDeliver.MailClient.Model;
using Fanex.MailDeliver.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.MailDeliver.Core.Business.Implement
{
    public class MailDeliverBusiness : IMailDeliverBusiness
    {
        private readonly IMailDeliverDataAccess mailDeliverDataAccess;
        private readonly IDynamicRepository mailDynamicRepository;
        private readonly MailExtension mailExtension;

        public MailDeliverBusiness(
            IMailDeliverDataAccess mailDeliverDataAccess,
            IDynamicRepository mailDynamicRepository,
            MailExtension mailExtension)

        {
            this.mailDeliverDataAccess = mailDeliverDataAccess;
            this.mailDynamicRepository = mailDynamicRepository;
            this.mailExtension = mailExtension;
        }

        public byte[] GetMailRecordByStream(long mailID)
        {
            try
            {
                if (mailID <= 0)
                {
                    return null;
                }

                var record = mailDeliverDataAccess.GetMailRecord(mailID, false);

                if (record != null)
                {
                    var stream = MailFormatter.Serialize(record);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        public MailRecord GetMailRecord(long mailID, bool isSend = false)
        {
            try
            {
                var mailRecord = mailDeliverDataAccess.GetMailRecord(mailID, isSend);

                if (isSend && mailRecord.IsEncrypt)
                {
                    mailRecord.MailTo = Cryptography.DecryptPassword(mailRecord.MailTo);
                    mailRecord.MailCc = Cryptography.DecryptPassword(mailRecord.MailCc);
                    mailRecord.MailBcc = Cryptography.DecryptPassword(mailRecord.MailBcc);
                    mailRecord.Body = Cryptography.DecryptPassword(mailRecord.Body);
                }

                return mailRecord;
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        public byte[] SaveMailConfig(MailConfig mailConfig)
        {
            try
            {
                var config = mailDeliverDataAccess.SaveMailConfig(mailConfig);
                if (config != null)
                {
                    MemInstance.Set<MemConfig>(config);
                }

                return MailFormatter.Serialize(config).ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        public IEnumerable<byte[]> GetListMailRecordByDate(
            int appID,
            DateTime mailDate,
            int page,
            int counter)
        {
            try
            {
                List<byte[]> mailList = new List<byte[]>();
                if (appID <= 0)
                {
                    return mailList;
                }

                var records = mailDeliverDataAccess.GetListMailRecordByDate(appID, mailDate, page, counter);
                if (records != null)
                {
                    foreach (var record in records)
                    {
                        var stream = MailFormatter.Serialize(record);
                        mailList.Add(stream.ToArray());
                    }
                }

                return mailList;
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        public IEnumerable<byte[]> GetListMailRecordByDateRange(
            int appID,
            DateTime fromDate,
            DateTime toDate,
            ref int totalRows,
            int page,
            int counter,
            int status)
        {
            try
            {
                List<byte[]> mailList = new List<byte[]>();
                if (appID < 0)
                {
                    return mailList;
                }

                var records = mailDeliverDataAccess.GetListMailRecordByDateRange(appID, fromDate, toDate, ref totalRows, page, counter, status);

                if (records != null)
                {
                    foreach (var record in records)
                    {
                        var stream = MailFormatter.Serialize(record);
                        mailList.Add(stream.ToArray());
                    }
                }

                return mailList;
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        public byte[] GetMailConfig(int appID)
        {
            try
            {
                var config = mailDeliverDataAccess.GetMailConfig(appID);
                if (config != null)
                {
                    var stream = MailFormatter.Serialize(config);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        #region mail setting

        public byte[] SaveMailSetting(MailSetting mailSetting)
        {
            try
            {
                var setting = mailDeliverDataAccess.SaveMailSetting(mailSetting);
                if (setting != null)
                {
                    MemInstance.Set<MemSetting>(setting);
                }

                return MailFormatter.Serialize(setting).ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        public byte[] GetMailSetting(int settingID)
        {
            try
            {
                if (settingID <= 0)
                {
                    return null;
                }

                var mailSetting = MemInstance.Get<MemSetting>(settingID) as MailSetting;
                if (mailSetting == null)
                {
                    mailSetting = mailDeliverDataAccess.GetMailSetting(settingID);
                }

                var stream = MailFormatter.Serialize(mailSetting);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        #endregion mail setting

        #region mail function

        public byte[] SaveMailFunction(MailFunction mailFunction)
        {
            try
            {
                var func = mailDeliverDataAccess.SaveMailFunction(mailFunction);
                if (func != null)
                {
                    MemInstance.Set<MemFunction>(func);
                }

                return MailFormatter.Serialize(func).ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        public byte[] GetMailFunction(int funcID)
        {
            try
            {
                if (funcID <= 0)
                {
                    return null;
                }

                var mailFunc = MemInstance.Get<MemFunction>(funcID) as MailFunction;
                if (mailFunc == null)
                {
                    mailFunc = mailDeliverDataAccess.GetMailFunction(funcID);
                }

                var stream = MailFormatter.Serialize(mailFunc);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        #endregion mail function

        #region get list for mem

        public IEnumerable<byte[]> GetListMailSetting()
        {
            List<byte[]> buffers = new List<byte[]>();
            try
            {
                var settings = mailDeliverDataAccess.GetListMailSetting();
                if (settings != null)
                {
                    foreach (var setting in settings)
                    {
                        var stream = MailFormatter.Serialize(setting);
                        buffers.Add(stream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return buffers;
        }

        public IEnumerable<byte[]> GetListMailConfig(string serverIp = "")
        {
            List<byte[]> buffers = new List<byte[]>();
            try
            {
                var configs = mailDeliverDataAccess.GetListMailConfig(serverIp);
                if (configs != null)
                {
                    foreach (var config in configs)
                    {
                        var stream = MailFormatter.Serialize(config);
                        buffers.Add(stream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return buffers;
        }

        public IEnumerable<byte[]> GetListMailFuntion()
        {
            List<byte[]> buffers = new List<byte[]>();
            try
            {
                var functions = mailDeliverDataAccess.GetListMailFuntion();
                if (functions != null)
                {
                    foreach (var function in functions)
                    {
                        var stream = MailFormatter.Serialize(function);
                        buffers.Add(stream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return buffers;
        }

        #endregion get list for mem

        public void SendMail(MailRecord record)
        {
            record = record.IsReady ? record : mailDeliverDataAccess.GetMailRecord(record.MailID, true);
            if (record != null && record.IsReady)
            {
                Task<string> task = Task.Factory.StartNew<string>(
                    () =>
                    {
                        string error = string.Empty;
                        mailExtension.SendMail(record, out error);
                        return error;
                    });

                string message = task.Result;
                LogExtension.LogMessage(string.Format("Sending mail: {0}. Error message: {1}", record.MailID, message));
            }
        }

        public IEnumerable<byte[]> GetListMailError(long mailID)
        {
            List<byte[]> errors = new List<byte[]>();
            try
            {
                var errorList = mailDynamicRepository.Fetch<MailError>(new GetMailErrorCriteria
                {
                    MailID = mailID
                });
                if (errorList != null && errorList.Any())
                {
                    foreach (var error in errorList)
                    {
                        var stream = MailFormatter.Serialize(error);
                        errors.Add(stream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return errors;
        }

        public byte[] SaveMailError(byte[] buffer)
        {
            return null;
        }
    }
}