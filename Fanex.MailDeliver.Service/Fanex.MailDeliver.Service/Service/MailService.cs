using Fanex.MailDeliver.Core.Business.Implement;
using Fanex.MailDeliver.Core.Business.Interface;
using Fanex.MailDeliver.Core.Business.Worker;
using Fanex.MailDeliver.Core.Infrastructure.Logging;
using Fanex.MailDeliver.Core.Infrastructure.Services;
using Fanex.MailDeliver.MailClient.Model;
using Fanex.MailDeliver.MailClient.ParamModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.MailDeliver.Service.Service
{
    public class MailService : IMailService
    {
        private readonly IMailDeliverBusiness mailDeliverBusiness;
        private readonly MailExtension mailExtension;
        private readonly SendMailWorker sendMailWorker;

        public MailService(
            IMailDeliverBusiness mailDeliverBusiness,
            MailExtension mailExtension,
            SendMailWorker sendMailWorker)
        {
            this.mailDeliverBusiness = mailDeliverBusiness;
            this.mailExtension = mailExtension;
            this.sendMailWorker = sendMailWorker;
        }

        public void SendMail(MailRecord mailRecord)
        {
            if (mailRecord != null)
            {
                mailRecord.ResentCount = AppConfigService.Config.NumberOfResendMail;
                mailExtension.AddMailToQueueForSendingMail(mailRecord);
            }
        }

        public byte[] StoreConfig(MailConfig mailConfig)
        {
            return mailDeliverBusiness.SaveMailConfig(mailConfig);
        }

        public byte[] GetMail(long mailID)
        {
            return mailDeliverBusiness.GetMailRecordByStream(mailID);
        }

        public IEnumerable<byte[]> GetListMail(GetListMailParam param)
        {
            return mailDeliverBusiness.GetListMailRecordByDate(param.AppId, param.MailDate, param.Page, param.Counter);
        }

        public Tuple<IEnumerable<byte[]>, int> GetListMailByDate(GetListMailByDateParam param)
        {
            var totalRows = 0;
            var result = mailDeliverBusiness.GetListMailRecordByDateRange(param.AppId, param.FromDate, param.ToDate, ref totalRows, param.Page, param.Counter, param.Status);
            return new Tuple<IEnumerable<byte[]>, int>(result, totalRows);
        }

        public byte[] GetConfig(int appID)
        {
            return mailDeliverBusiness.GetMailConfig(appID);
        }

        public byte[] StoreSetting(MailSetting mailSetting)
        {
            return mailDeliverBusiness.SaveMailSetting(mailSetting);
        }

        public byte[] GetSetting(int settingID)
        {
            return mailDeliverBusiness.GetMailSetting(settingID);
        }

        /// <summary>
        /// Resend mail when mail failed.
        /// </summary>
        /// <param name="mailID">Mail id need be resent.</param>
        public void ReSendMail(long mailID)
        {
            try
            {
                lock (AppConfigService.ListMailLockObject)
                {
                    if (AppConfigService.ListMailFailed.Any())
                    {
                        MailRecord mailRemoveFromResendList = null;
                        foreach (var mail in AppConfigService.ListMailFailed)
                        {
                            if (mail.MailID == mailID)
                            {
                                mailRemoveFromResendList = mail;
                                break;
                            }
                        }

                        if (mailRemoveFromResendList != null)
                        {
                            AppConfigService.ListMailFailed.Remove(mailRemoveFromResendList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            sendMailWorker.SendMail(
                new MailRecord
                {
                    MailID = mailID,
                    IsReady = false
                });
        }

        #region mail function service

        public byte[] StoreFunction(MailFunction mailFunction)
        {
            return mailDeliverBusiness.SaveMailFunction(mailFunction);
        }

        public byte[] GetFunction(int funcID)
        {
            return mailDeliverBusiness.GetMailFunction(funcID);
        }

        #endregion mail function service

        #region get list

        public IEnumerable<byte[]> GetListSetting()
        {
            return mailDeliverBusiness.GetListMailSetting();
        }

        public IEnumerable<byte[]> GetListConfig(string serverIp)
        {
            return mailDeliverBusiness.GetListMailConfig(serverIp);
        }

        public IEnumerable<byte[]> GetListFuntion()
        {
            return mailDeliverBusiness.GetListMailFuntion();
        }

        public IEnumerable<byte[]> GetListError(long mailID)
        {
            return mailDeliverBusiness.GetListMailError(mailID);
        }

        #endregion get list
    }
}