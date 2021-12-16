// --------------------------------------------------------------------------
// <copyright file="SendMailWorker.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.Business.Worker
{
    using Fanex.MailDeliver.Business;
    using Fanex.MailDeliver.Business.Interface;
    using Fanex.MailDeliver.Core.Business;
    using Fanex.MailDeliver.Core.Infrastructure.Logging;
    using Fanex.MailDeliver.Model;
    using System;
    using System.Threading;

    public interface SendMailWorker
    {
        void Run(MailRecord mailRecord);

        void SendMail(MailRecord record);
    }

    public class SendMailWorkerImp : SendMailWorker
    {
        private readonly IMailDeliverBusiness mailDeliverBusiness;
        private readonly MailExtension mailExtension;
        private readonly DataService dataService;
        private bool isBreak = false;

        public SendMailWorkerImp(
            IMailDeliverBusiness mailDeliverBusiness,
            MailExtension mailExtension,
            DataService dataService)
        {
            this.mailDeliverBusiness = mailDeliverBusiness;
            this.mailExtension = mailExtension;
            this.dataService = dataService;
        }

        public bool IsBreak
        {
            get
            {
                return isBreak;
            }

            set
            {
                isBreak = value;
            }
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
                                LogExtension.LogMessage(string.Format("Got Email from RabbitMQ: {0}", data.Subject));
                                var mailId = 0;

                                try
                                {
                                    mailId = (int)dataService.InsertMailRecord(data);
                                    if (mailId > 0)
                                    {
                                        LogExtension.LogMessage(string.Format("Stored mail {0}", mailId));
                                        data.IsReady = true;
                                        data.MailID = mailId;
                                        SendMail(data);
                                    }
                                    else
                                    {
                                        LogExtension.LogMessage("Store mail failed.");
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
                string error = string.Empty;
                mailExtension.SendMail(record, out error);
                LogExtension.LogMessage(string.Format("Sending mail: {0}.", record.MailID));
            }
        }
    }
}