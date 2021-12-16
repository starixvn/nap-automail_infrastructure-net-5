namespace Fanex.MailDeliver.Core.Business.Worker
{
    using Fanex.MailDeliver.Business;
    using Fanex.MailDeliver.Core.Infrastructure.Logging;
    using Fanex.MailDeliver.Core.Infrastructure.Services;
    using Fanex.MailDeliver.Model;
    using System;
    using System.Collections.Generic;

    public interface ResendMailWorker
    {
        void Run();
    }

    public class ResendMailWorkerImp : ResendMailWorker
    {
        private readonly MailExtension mailExtension;

        public ResendMailWorkerImp(MailExtension mailExtension)
        {
            this.mailExtension = mailExtension;
        }

        public void Run()
        {
            try
            {
                if (AppConfigService.ListMailFailed.Count > 0)
                {
                    List<MailRecord> listMailFailCopied = null;
                    lock (AppConfigService.ListMailLockObject)
                    {
                        listMailFailCopied = new List<MailRecord>(AppConfigService.ListMailFailed);
                        AppConfigService.ListMailFailed.Clear();
                    }

                    if (listMailFailCopied != null && listMailFailCopied.Count > 0)
                    {
                        for (int mailIndex = 0; mailIndex < listMailFailCopied.Count; mailIndex++)
                        {
                            MailRecord currentMail = listMailFailCopied[mailIndex];
                            if (currentMail.ResentCount > 0)
                            {
                                listMailFailCopied[mailIndex].ResentCount--;
                                mailExtension.AddMailToQueueForSendingMail(currentMail);
                            }
                        }

                        listMailFailCopied = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }
    }
}