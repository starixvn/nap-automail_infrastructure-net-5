using EasyNetQ;
using Fanex.Caching;
using Fanex.MailDelivery.Core;
using Fanex.MailDelivery.Core.Messaging;
using Fanex.MailDelivery.Core.Models;
using Fanex.MailDelivery.Services.Services;
using System;
using System.Threading.Tasks;

namespace Fanex.MailStation.Service.MessageBrokers
{
    public interface MessageBroker : IDisposable
    {
        void Start();
    }

    public class MessageBrokerImpl : MessageBroker
    {
        private IBus bus;
        private readonly MailService mailService;
        private readonly LogService logService;
        private readonly UMService umService;
        private const int waitingForRabbitMQInSeconds = 5;
        private readonly ICacheService cacheService;
        private const string UMInfoCacheKey = "MailStation.UMInfoCacheKey";

        public MessageBrokerImpl(
            MailService mailService,
            LogService logService,
            UMService umService,
            IBus bus,
            ICacheService cacheService)
        {
            this.logService = logService;
            this.mailService = mailService;
            this.umService = umService;
            this.bus = bus;
            this.cacheService = cacheService;
        }

        public void Start()
        {
            bus = bus ?? RabbitHutch.CreateBus(AppConfigService.AppConfig.RabbitMQConnectionString);

            SubscribeMail();
            SubscribeJobResult();
            SubscribeTaskLogMessage();
            SubscribeClientNotificationMessage();

            ResponseGetScheduleList();
            RespondGetMailTaskForById();
            RespondAddMailTaskForRunningJob();
            RespondUMInfo();
        }

        #region Subscribe

        private void SubscribeMail()
        {
            bool isFailed = false;

            try
            {
                bus.PubSub.Subscribe<Mail>(
                    MailDeliveryConstants.SendMailRoutingKey,
                     (mail) =>
                    {
                        umService.WaitForUMOff();

                        mailService.SendMail(mail);
                    },
                    config => config.WithTopic(MailDeliveryConstants.SendMailRoutingKey));

                logService.LogInfo($"Subcribed the queue with key {MailDeliveryConstants.SendMailRoutingKey} successfully");
            }
            catch (Exception ex)
            {
                logService.LogError($"There is an error while subscribing the queue with key {MailDeliveryConstants.SendMailRoutingKey}. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed)
            {
                SubscribeMail();
            }
        }

        private void SubscribeClientNotificationMessage()
        {
            bool isFailed = false;

            try
            {
                bus.PubSub.Subscribe<UpdateActiveTimeRequest>(
                    MailDeliveryConstants.ClientNotificationTopic,
                    clientInfoMessage =>
                    {
                        mailService.UpdateClientActiveTime(clientInfoMessage);
                    },
                    config => config.WithTopic(MailDeliveryConstants.ClientNotificationTopic));

                logService.LogInfo($"Subcribed the queue with key {MailDeliveryConstants.ClientNotificationTopic} successfully");
            }
            catch (Exception ex)
            {
                logService.LogError($"There is an error while subscribing the queue with key {MailDeliveryConstants.ClientNotificationTopic}. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed)
            {
                SubscribeClientNotificationMessage();
            }
        }

        private void SubscribeJobResult()
        {
            bool isFailed = false;

            try
            {
                bus.PubSub.Subscribe<JobMessage>(
                    MailDeliveryConstants.JobResultRoutingKey,
                     (jobStatus) =>
                    {
                        umService.WaitForUMOff();

                        mailService.UpdateJobStatus(jobStatus);
                    },
                    config => config.WithTopic(MailDeliveryConstants.JobResultRoutingKey));

                logService.LogInfo($"Subcribed the queue with key {MailDeliveryConstants.JobResultRoutingKey} successfully");
            }
            catch (Exception ex)
            {
                logService.LogError($"There is an error while subscribing the queue with key {MailDeliveryConstants.JobResultRoutingKey}. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed)
            {
                SubscribeJobResult();
            }
        }

        private void SubscribeTaskLogMessage()
        {
            bool isFailed = false;

            try
            {
                bus.PubSub.Subscribe<TaskLog>(
                    MailDeliveryConstants.TaskLogRoutingKey,
                    log =>
                    {
                        mailService.InsertTaskLog(log);
                    },
                    config => config.WithTopic(MailDeliveryConstants.TaskLogRoutingKey));

                logService.LogInfo($"Subcribed the queue with key {MailDeliveryConstants.TaskLogRoutingKey} successfully");
            }
            catch (Exception ex)
            {
                logService.LogError($"There is an error while subscribing the queue with key {MailDeliveryConstants.TaskLogRoutingKey}. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed)
            {
                SubscribeTaskLogMessage();
            }
        }

        #endregion Subscribe

        #region Response

        private void ResponseGetScheduleList()
        {
            bool isFailed = false;

            try
            {
                bus.Rpc.Respond<GetScheduleListRequest, GetScheduleListResponse>(request => Task.FromResult(mailService.GetScheduleList(request)));

                logService.LogInfo($"Registered to response schedules to clients successfully");
            }
            catch (Exception ex)
            {
                logService.LogError($"There is an error while registering to response schedules to clients. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed)
            {
                ResponseGetScheduleList();
            }
        }

        private void RespondAddMailTaskForRunningJob()
        {
            bool isFailed = false;

            try
            {
                bus.Rpc.Respond<JobMessage, JobMessage>(
                    jobMessage =>
                    {
                        jobMessage.Schedule.TaskId = mailService.AddMailTaskForRunningJob(jobMessage);

                        return Task.FromResult(jobMessage);
                    });

                logService.LogInfo($"Registered to add mail tasks for running jobs successfully");
            }
            catch (Exception ex)
            {
                logService.LogError($"There is an error while registering to add mail tasks for running jobs. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed)
            {
                RespondAddMailTaskForRunningJob();
            }
        }

        private void RespondUMInfo()
        {
            bool isFailed = false;

            try
            {
                bus.Rpc.Respond<UMInfoModel, UMInfoModel>(async (x) =>
                {
                    var umInfo = cacheService.Get<UMInfoModel>(UMInfoCacheKey);

                    if (umInfo == null)
                    {
                        umInfo = await umService.GetUMInfo();

                        if (umInfo != null && umInfo.IsUnderMaintenanceTime)
                        {
                            cacheService.Set(
                                UMInfoCacheKey,
                                umInfo,
                                new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(AppConfigService.AppConfig.UMInfoCachingTimeoutInSeconds)));
                        }
                    }

                    return umInfo;
                });

                logService.LogInfo($"Registered to respond the UM info to clients successfully");
            }
            catch (Exception ex)
            {
                logService.LogError($"There is an error while registering to respond the UM info to clients. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed)
            {
                RespondUMInfo();
            }
        }

        private void RespondGetMailTaskForById()
        {
            bool isFailed = false;

            try
            {
                bus.Rpc.Respond<GetMailTaskByIdRequest, MailTaskModel>(request => Task.FromResult(mailService.GetMailTaskById(request)));

                logService.LogInfo($"Registered to response mail tasks to clients successfully");
            }
            catch (Exception ex)
            {
                logService.LogError($"There is an error while registering to response mail tasks to clients. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed)
            {
                RespondGetMailTaskForById();
            }
        }

        #endregion Response

        #region IDisposable Support

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //  dispose managed state (managed objects).
                }

                bus?.Dispose();

                disposedValue = true;
            }
        }

        ~MessageBrokerImpl()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}