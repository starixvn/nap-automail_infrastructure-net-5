using Fanex.MailDeliver.Core.Business.Implement;
using Fanex.MailDeliver.MailClient.Model;

namespace Fanex.MailDeliver.Core.Business.Worker;

using Fanex.MailDeliver.Core.Infrastructure.Logging;
using Infrastructure.Services;
using System;
using System.Collections.Generic;

public interface ResendMailWorker
{
    void Run();
}

public class ResendMailWorkerImp(MailExtension mailExtension) : ResendMailWorker
{
    public void Run()
    {
        try
        {
            if (AppConfigService.ListMailFailed.Count > 0)
            {
                List<MailRecord> listMailFailCopied = null;
                lock (AppConfigService.ListMailLockObject)
                {
                    listMailFailCopied = [..AppConfigService.ListMailFailed];
                    AppConfigService.ListMailFailed.Clear();
                }

                if (listMailFailCopied != null && listMailFailCopied.Count > 0)
                {
                    for (var mailIndex = 0; mailIndex < listMailFailCopied.Count; mailIndex++)
                    {
                        var currentMail = listMailFailCopied[mailIndex];
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