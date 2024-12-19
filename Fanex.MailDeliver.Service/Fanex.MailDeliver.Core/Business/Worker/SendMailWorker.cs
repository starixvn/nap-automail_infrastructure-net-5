// --------------------------------------------------------------------------
// <copyright file="SendMailWorker.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

using Fanex.MailDeliver.Core.Business.Implement;
using Fanex.MailDeliver.Core.Business.Interface;
using Fanex.MailDeliver.Core.Infrastructure.Logging;
using Fanex.MailDeliver.MailClient.Model;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace Fanex.MailDeliver.Core.Business.Worker;

public interface SendMailWorker
{
    void Run(MailRecord mailRecord);

    void SendMail(MailRecord record);
}

public class SendMailWorkerImp(
    IMailDeliverBusiness mailDeliverBusiness,
    MailExtension mailExtension,
    DataService dataService)
    : SendMailWorker
{
    private bool isBreak;

    public bool IsBreak
    {
        get => isBreak;

        set => isBreak = value;
    }

    public void Run(MailRecord mailRecord)
    {
        try
        {
            MailRecord data = null;
            {
                try
                {
                    data = mailRecord;
                    if (data != null)
                    {
                        if (data.MailID > 0)
                        {
                            data.IsReady = true;
                            SendMail(data);
                        }
                        else
                        {
                            LogExtension.LogMessage($"Got Email from RabbitMQ: {data.Subject}");
                            var mailId = 0;

                            try
                            {
                                mailId = (int)dataService.InsertMailRecord(data);
                                if (mailId > 0)
                                {
                                    LogExtension.LogMessage($"Stored mail {mailId}");
                                    data.IsReady = true;
                                    data.MailID = mailId;
                                    SendMail(data);
                                }
                                else
                                {
                                    var mailJson = JsonConvert.SerializeObject(mailRecord, Formatting.Indented);
                                    LogExtension.LogMessage($"Store mail failed - {mailJson}");
                                }
                            }
                            catch (Exception ex)
                            {
                                LogExtension.LogError(ex);
                                data.IsReady = true;
                                SendMail(data);
                                Thread.Sleep(5000);
                            }

                            Thread.Sleep(1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogExtension.LogError(ex);
                    Thread.Sleep(5000);
                }
            }

            GC.Collect();
        }
        catch (Exception ex)
        {
            LogExtension.LogError(ex);
        }
    }

    public void SendMail(MailRecord record)
    {
        record = record.IsReady ? record : mailDeliverBusiness.GetMailRecord(record.MailID, true);
        if (record != null && record.IsReady)
        {
            var error = string.Empty;
            mailExtension.SendMail(record, out error);
            LogExtension.LogMessage($"Sending mail: {record.MailID}.");
        }
        else
        {
            var jsonMail = JsonConvert.SerializeObject(record, Formatting.Indented);
            LogExtension.LogMessage($"Mail can't be sent because it's null or not ready. {jsonMail}");
        }
    }
}