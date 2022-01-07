using Fanex.MailDeliver.Core.Criterias;
using Fanex.MailDeliver.Core.Infrastructure.Logging;
using Fanex.MailDeliver.DataAccess.Interface;
using Fanex.MailDeliver.Model;
using System;

namespace Fanex.MailDeliver.Core.Business
{
    public interface DataService
    {
        long InsertMailRecord(MailRecord mailRecord);

        void UpdateMailStatus(long mailID, bool status);

        long InsertMailError(InsertMailErrorCriteria criteria);
    }

    public class DataServiceImp : DataService
    {
        private readonly IMailDeliverDataAccess mailDeliverDataAccess;

        public DataServiceImp(IMailDeliverDataAccess mailDeliverDataAccess)
        {
            this.mailDeliverDataAccess = mailDeliverDataAccess;
        }

        public long InsertMailRecord(MailRecord mailRecord)
        {
            try
            {
                if ((mailRecord.Config != null && mailRecord.Config.AppID <= 0) && mailRecord.AppID <= 0)
                {
                    return 0;
                }

                var currentAppId = mailRecord.Config != null ? mailRecord.Config.AppID : mailRecord.AppID;
                var currentApp = mailDeliverDataAccess.GetMailConfig(currentAppId);
                if (currentApp != null && currentApp.IsEncrypt)
                {
                    mailRecord.IsEncrypt = currentApp.IsEncrypt;
                }

                return mailDeliverDataAccess.InsertMailRecord(mailRecord);
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return 0;
        }

        public void UpdateMailStatus(long mailID, bool status)
        {
            try
            {
                mailDeliverDataAccess.UpdateMailStatus(mailID, status);
                LogExtension.LogMessage(string.Format("Sent mail {0}. Status {1}", mailID, status));
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        public long InsertMailError(InsertMailErrorCriteria criteria)
        {
            try
            {
                return mailDeliverDataAccess.InsertMailError(criteria);
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
                return 0;
            }
        }
    }
}