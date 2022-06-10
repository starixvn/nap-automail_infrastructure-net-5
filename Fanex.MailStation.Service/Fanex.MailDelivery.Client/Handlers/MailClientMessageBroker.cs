using EasyNetQ;
using EasyNetQ.Logging;
using Fanex.Caching;
using Fanex.MailDelivery.Client.Configurations;
using Fanex.MailDelivery.Client.Logging;
using Fanex.MailDelivery.Core;
using Fanex.MailDelivery.Core.Messaging;
using Fanex.MailDelivery.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fanex.MailDelivery.Client.Handlers
{
    public interface IMailClientMessageBroker
    {
        IJobHandler JobHandler { get; }

        Task StartAsync();

        Task StopAsync();

        Task SendFunctionMailAsync(Mail mail);

        Task<IList<Schedule>> GetSchedulesAsync();

        Task<Schedule> GetScheduleAsync(int mailId);

        Task<UMInfoModel> GetUMInfoAsync(int retryCount = 0);

        Task PublishClientNotifyMessageAsync();

        Task ReportJobAsync(JobMessage jobMessage);

        Task<JobMessage> RequestAddMailTaskForRunningJobAsync(JobMessage jobMessage, int retryCount = 0);

        Task<MailTaskModel> RequestMailTaskByIdAsync(GetMailTaskByIdRequest request, int retryCount = 0);

        Task PublishRunAfterUMJobAsync(Schedule schedule, int retryCount = 0);

        Task PublishTaskLogAsync(TaskLog taskLog, int retryCount = 0);

        Task UpdateSchedulesAsync(IList<Schedule> schedules);
    }

    public class MailClientMessageBroker : IMailClientMessageBroker
    {
        private readonly string updateScheduleSubscriptionId;
        private readonly string retryJobSubscriptionId;
        private readonly string runNowJobSubscriptionId;
        private readonly string runAfterUMJobSubscriptionId;
        private readonly IBus bus;
        private readonly IJobHandler jobHandler;
        private readonly int waitingForRabbitMQInSeconds;
        private readonly ICacheService cacheService;
        private const string RunAfterUMScheduleKey = "MailStation.RunAfterUMScheduleKey";
        private readonly MailStationSetting mailStationSetting;

        public MailClientMessageBroker(MailStationSetting mailStationSetting) : this(null, null, null, mailStationSetting)
        {
        }

        public MailClientMessageBroker(IJobHandler jobHandler, IBus bus, ICacheService cacheService, MailStationSetting mailStationSetting)
        {
            LogProvider.SetCurrentLogProvider(new RabbitMQLogProvider());

            this.mailStationSetting = mailStationSetting;
            this.jobHandler = jobHandler ?? new JobHandler(mailStationSetting);
            this.bus = bus ?? RabbitHutch.CreateBus(mailStationSetting.RabbitMQConnectionString);
            this.cacheService = cacheService ?? new CacheService();

            updateScheduleSubscriptionId = string.Format(MailDeliveryConstants.UpdateScheduleRoutingKeyFormat, mailStationSetting.SiteId);
            retryJobSubscriptionId = string.Format(MailDeliveryConstants.RetryJobRoutingKeyFormat, mailStationSetting.SiteId);
            runNowJobSubscriptionId = string.Format(MailDeliveryConstants.RunNowJobRoutingKeyFormat, mailStationSetting.SiteId);
            runAfterUMJobSubscriptionId = string.Format(MailDeliveryConstants.RunAfterUMJobRoutingKeyFormat, mailStationSetting.SiteId);
            waitingForRabbitMQInSeconds = mailStationSetting.RequestRetryIntervalInSeconds;
        }

        public IJobHandler JobHandler => jobHandler;

        public async Task StartAsync()
        {
            await jobHandler.InitializeSchedulerAsync();

            while (!bus.IsConnected)
            {
                await Task.Delay(TimeSpan.FromSeconds(waitingForRabbitMQInSeconds));
            }

            if (mailStationSetting.HasSchedule())
            {
                await SubscribeScheduleChangeAsync();
                await SubscribeRunNowJobAsync();
                await SubscribeRetryJobAsync();
                await SubscribeRunAfterUMJobAsync();
            }

            await jobHandler.StartAsync();
        }

        public async Task StopAsync()
        {
            bus.Dispose();

            await jobHandler.StopAsync();
        }

        #region PublishAsync
        public async Task SendFunctionMailAsync(Mail mail)
        {
            if (mail.ApplicationSendMailId == 0)
            {
                mail.ApplicationSendMailId = mailStationSetting.ApplicationSendMailId;
            }

            mail.SiteId = mailStationSetting.SiteId;

            await PublishMailAsync(mail);
        }

        public async Task PublishClientNotifyMessageAsync()
        {
            var schedules = mailStationSetting.Schedules;
            string mailIds = string.Join(",", schedules.Select(x => x.Id));

            await bus.PublishAsync(new UpdateActiveTimeRequest { MailIds = mailIds }, MailDeliveryConstants.ClientNotificationTopic);
        }

        public async Task ReportJobAsync(JobMessage jobMessage)
        {
            if (jobMessage.Mail != null && jobMessage.Mail.ApplicationSendMailId == 0)
            {
                jobMessage.Mail.ApplicationSendMailId = mailStationSetting.ApplicationSendMailId;
            }

            await PublishJobMessageAsync(jobMessage);
        }

        private async Task PublishMailAsync(Mail mail, int retryCount = 0)
        {
            bool isFailed = false;

            try
            {
                await bus.PublishAsync(mail, MailDeliveryConstants.SendMailRoutingKey);
            }
            catch (Exception ex)
            {
                LogService.LogError($"There is an error while sending a function mail to Mail Station server. Retrying in {waitingForRabbitMQInSeconds} seconds...", $"{mail.MailingListId}", ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                await Task.Delay(TimeSpan.FromSeconds(waitingForRabbitMQInSeconds));

                await PublishMailAsync(mail, retryCount);
            }
        }

        private async Task PublishJobMessageAsync(JobMessage jobMessage, int retryCount = 0)
        {
            bool isFailed = false;

            try
            {
                await bus.PublishAsync(jobMessage, MailDeliveryConstants.JobResultRoutingKey);
            }
            catch (Exception ex)
            {
                LogService.LogError($@"There is an error while reporting the status of the scheduled mail to Mail Station server. Retrying in {
                                        waitingForRabbitMQInSeconds
                                        } seconds...", jobMessage.Schedule, ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                await Task.Delay(TimeSpan.FromSeconds(waitingForRabbitMQInSeconds));

                await PublishJobMessageAsync(jobMessage, retryCount);
            }
        }

        public async Task PublishRunAfterUMJobAsync(Schedule schedule, int retryCount = 0)
        {
            bool isFailed = false;

            try
            {
                await bus.PublishAsync(schedule, runAfterUMJobSubscriptionId);
            }
            catch (Exception ex)
            {
                LogService.LogError($"There is an error while registering to run this job after UM. Retrying in {waitingForRabbitMQInSeconds} seconds...", schedule, ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                await Task.Delay(TimeSpan.FromSeconds(waitingForRabbitMQInSeconds));

                await PublishRunAfterUMJobAsync(schedule, retryCount);
            }
        }

        public async Task PublishTaskLogAsync(TaskLog taskLog, int retryCount = 0)
        {
            bool isFailed = false;

            try
            {
                await bus.PublishAsync(taskLog, MailDeliveryConstants.TaskLogRoutingKey);
            }
            catch (Exception ex)
            {
                LogService.LogError($"There is an error while sending a task log to Mail Station server for inserting. Retrying in {waitingForRabbitMQInSeconds} seconds...", ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                await Task.Delay(TimeSpan.FromSeconds(waitingForRabbitMQInSeconds));

                await PublishTaskLogAsync(taskLog, retryCount);
            }
        }
        #endregion

        #region RequestAsync
        public async Task<IList<Schedule>> GetSchedulesAsync()
        {
            var schedules = mailStationSetting.Schedules;
            var request = new GetScheduleListRequest();

            foreach (ScheduleSetting scheduleConfig in schedules)
            {
                request.MailIds.Add(scheduleConfig.Id);
            }

            GetScheduleListResponse response = await RequestMailSchedules(request);

            return response?.Schedules;
        }

        public async Task<Schedule> GetScheduleAsync(int mailId)
        {
            var request = new GetScheduleListRequest { MailIds = new List<int> { mailId } };

            GetScheduleListResponse response = await RequestMailSchedules(request);

            return response?.Schedules.FirstOrDefault();
        }

        public async Task<JobMessage> RequestAddMailTaskForRunningJobAsync(JobMessage jobMessage, int retryCount = 0)
        {
            JobMessage result = null;
            bool isFailed = false;

            try
            {
                result = await bus.RequestAsync<JobMessage, JobMessage>(jobMessage);
            }
            catch (Exception ex)
            {
                LogService.LogError($"There is an error while sending a request to add a running mail task. Retrying...", jobMessage.Schedule, ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                result = await RequestAddMailTaskForRunningJobAsync(jobMessage, retryCount);
            }

            return result;
        }

        public async Task<MailTaskModel> RequestMailTaskByIdAsync(GetMailTaskByIdRequest request, int retryCount = 0)
        {
            MailTaskModel result = null;
            bool isFailed = false;

            try
            {
                result = await bus.RequestAsync<GetMailTaskByIdRequest, MailTaskModel>(request);
            }
            catch (Exception ex)
            {
                LogService.LogError($"Client cannot get the mail task #{request.TaskId} from Mail Station server. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                result = await RequestMailTaskByIdAsync(request, retryCount);
            }

            return result;
        }

        public async Task<UMInfoModel> GetUMInfoAsync(int retryCount = 0)
        {
            UMInfoModel result = null;
            bool isFailed = false;

            try
            {
                result = await bus.RequestAsync<UMInfoModel, UMInfoModel>(new UMInfoModel());
            }
            catch (Exception ex)
            {
                LogService.LogError($"Client cannot get the UM info from Mail Station server. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                result = await GetUMInfoAsync(retryCount);
            }

            return result;
        }

        private async Task<GetScheduleListResponse> RequestMailSchedules(GetScheduleListRequest request, int retryCount = 0)
        {
            GetScheduleListResponse response = null;
            bool isFailed = false;

            try
            {
                response = await bus.RequestAsync<GetScheduleListRequest, GetScheduleListResponse>(request);
            }
            catch (Exception ex)
            {
                LogService.LogError($"Client cannot get schedules from Mail Station server. Retrying...", string.Join(", ", request.MailIds), ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                response = await RequestMailSchedules(request, retryCount);
            }

            return response;
        }
        #endregion

        #region Subscribe
        public async Task SubscribeScheduleChangeAsync(int retryCount = 0)
        {
            bool isFailed = false;

            try
            {
                bus.SubscribeAsync<Schedule>(
                    GetSubscriptionIdKey(updateScheduleSubscriptionId),
                    schedule => jobHandler.UpdateScheduleAsync(schedule),
                    config => config.WithTopic(updateScheduleSubscriptionId));

                LogService.LogInfo($"Subcribed the queue with key {updateScheduleSubscriptionId} successfully");
            }
            catch (Exception ex)
            {
                LogService.LogError($"There is an error while subscribing a queue with key {updateScheduleSubscriptionId}. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                await SubscribeScheduleChangeAsync(retryCount);
            }
        }

        public async Task SubscribeRunNowJobAsync(int retryCount = 0)
        {
            bool isFailed = false;

            try
            {
                bus.SubscribeAsync<Schedule>(
                    GetSubscriptionIdKey(runNowJobSubscriptionId),
                    schedule => RetryAllProcessAsync(schedule),
                    config => config.WithTopic(runNowJobSubscriptionId));

                LogService.LogInfo($"Subcribed the queue with key {runNowJobSubscriptionId} successfully");
            }
            catch (Exception ex)
            {
                LogService.LogError($"There is an error while subscribing a queue with key {runNowJobSubscriptionId}. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                await SubscribeRunNowJobAsync(retryCount);
            }
        }

        public async Task SubscribeRetryJobAsync(int retryCount = 0)
        {
            bool isFailed = false;

            try
            {
                bus.SubscribeAsync<Schedule>(
                    GetSubscriptionIdKey(retryJobSubscriptionId),
                    schedule => RetryAllProcessAsync(schedule),
                    config => config.WithTopic(retryJobSubscriptionId));

                LogService.LogInfo($"Subcribed the queue with key {retryJobSubscriptionId} successfully");
            }
            catch (Exception ex)
            {
                LogService.LogError($"There is an error while subscribing a queue with key {retryJobSubscriptionId}. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                await SubscribeRetryJobAsync(retryCount);
            }
        }

        public async Task SubscribeRunAfterUMJobAsync(int retryCount = 0)
        {
            bool isFailed = false;

            try
            {
                bus.SubscribeAsync<Schedule>(
                    GetSubscriptionIdKey(runAfterUMJobSubscriptionId),
                    schedule => RunAfterUMJobAsync(schedule),
                    config => config.WithTopic(runAfterUMJobSubscriptionId).WithPrefetchCount(1));

                LogService.LogInfo($"Subcribed the queue with key {runAfterUMJobSubscriptionId} successfully");
            }
            catch (Exception ex)
            {
                LogService.LogError($"There is an error while subscribing a queue with key {runAfterUMJobSubscriptionId}. Retrying...", ex);
                isFailed = true;
            }

            if (isFailed && retryCount < mailStationSetting.RequestRetryLimit)
            {
                retryCount++;

                await SubscribeRunAfterUMJobAsync(retryCount);
            }
        }
        #endregion

        #region Job tasks
        public async Task UpdateSchedulesAsync(IList<Schedule> schedules)
        {
            if (schedules != null)
            {
                foreach (var schedule in schedules)
                {
                    await jobHandler.UpdateScheduleAsync(schedule);
                }
            }
        }
        #endregion

        private async Task RunAfterUMJobAsync(Schedule schedule)
        {
            if (mailStationSetting.HasScheduleId(schedule.Id))
            {
                var umInfo = await GetUMInfoAsync();

                if (umInfo != null && umInfo.IsUnderMaintenanceTime)
                {
                    LogService.LogInfo($"UM Info: {JsonConvert.SerializeObject(umInfo)}");

                    var afterUMTime = new DateTime(umInfo.To.Ticks, DateTimeKind.Utc).AddMinutes(1);

                    TimeSpan waitingTime = afterUMTime - DateTime.UtcNow;

                    if (waitingTime.TotalSeconds > 0)
                    {
                        LogService.LogInfo($"Waiting for UM time in {(int)waitingTime.TotalMinutes} minutes.");

                        await Task.Delay(waitingTime);
                    }
                }

                var runScheduleCacheKey = $"{RunAfterUMScheduleKey}{schedule.Id}";
                var runSchedule = cacheService.Get<Schedule>(runScheduleCacheKey);

                if (runSchedule == null)
                {
                    await jobHandler.RetryAllProcessAsync(schedule);

                    cacheService.Set(
                                runScheduleCacheKey,
                                schedule,
                                new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(3)));
                }
            }
        }

        private async Task RetryAllProcessAsync(Schedule schedule)
        {
            if (mailStationSetting.HasScheduleId(schedule.Id))
            {
                LogService.LogInfo($"Mail #{schedule.Id} ({schedule.Name}) is triggered to start now from management site (mSite)");

                await jobHandler.RetryAllProcessAsync(schedule);
            }
        }

        private static string GetSubscriptionIdKey(string topic)
        {
            var ip = (
                from ips in Dns.GetHostEntry(Dns.GetHostName()).AddressList
                where ips.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                select ips)
                .FirstOrDefault();

            return string.Format("{0}_{1}_{2}", topic, Process.GetProcessById(Process.GetCurrentProcess().Id).ProcessName, ip);
        }
    }
}