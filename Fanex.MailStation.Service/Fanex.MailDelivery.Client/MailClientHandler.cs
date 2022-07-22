using Fanex.MailDelivery.Client.Configurations;
using Fanex.MailDelivery.Client.Handlers;
using Fanex.MailDelivery.Core.Messaging;
using Fanex.MailDelivery.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

#if !NETFRAMEWORK

using Microsoft.Extensions.Configuration;

#endif

namespace Fanex.MailDelivery.Client
{
    public static class MailClientHandler
    {
        private static IMailClientMessageBroker messageBroker;

        public static MailStationSetting MailStationSetting { get; private set; }

#if NETFRAMEWORK
        public static void Start()
        {
            StartAsync()
                .GetAwaiter()
                .GetResult();
        }

        public static async Task StartAsync()
        {
            MailStationSetting = new MailStationSetting();
            var broker = new MailClientMessageBroker(MailStationSetting);

            await StartAsync(broker);
        }
#else

        public static void Start(string appSettingFileName = "appsettings.json")
        {
            StartAsync(appSettingFileName)
                .GetAwaiter()
                .GetResult();
        }

        public static async Task StartAsync(string appSettingFileName = "appsettings.json")
        {
            var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appSettingFileName);

            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"{fileName} not found.");
            }

            var configuration = new ConfigurationBuilder()
                                  .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                  .AddJsonFile(appSettingFileName, false, false)
                                  .Build();
            MailStationSetting = configuration.GetSection("Fanex.MailDelivery.Client").Get<MailStationSetting>();

            var broker = new MailClientMessageBroker(MailStationSetting);

            await StartAsync(broker);
        }

#endif

        public static async Task StartAsync(IMailClientMessageBroker messageBroker)
        {
            MailClientHandler.messageBroker = messageBroker;

            await MailClientHandler.messageBroker.StartAsync();
        }

        public static void Stop()
        {
            StopAsync()
                .GetAwaiter()
                .GetResult();
        }

        public static async Task StopAsync()
        {
            await messageBroker?.StopAsync();
        }

        public static void SendFunctionMail(Mail mail)
        {
            SendFunctionMailAsync(mail)
                .GetAwaiter()
                .GetResult();
        }

        public static async Task SendFunctionMailAsync(Mail mail)
        {
            await messageBroker.SendFunctionMailAsync(mail);
        }

        public static async Task<IList<Schedule>> GetSchedulesAsync()
        {
            return await messageBroker.GetSchedulesAsync();
        }

        internal static async Task<Schedule> GetScheduleAsync(int mailId)
        {
            return await messageBroker.GetScheduleAsync(mailId);
        }

        internal static async Task PublishClientNotifyMessageAsync()
        {
            await messageBroker.PublishClientNotifyMessageAsync();
        }

        internal static async Task<JobMessage> RequestAddMailTaskForRunningJobAsync(JobMessage jobResult)
        {
            return await messageBroker.RequestAddMailTaskForRunningJobAsync(jobResult);
        }

        internal static async Task<MailTaskModel> RequestMailTaskByIdAsync(GetMailTaskByIdRequest taskID)
        {
            return await messageBroker.RequestMailTaskByIdAsync(taskID);
        }

        internal static async Task ReportJobAsync(JobMessage jobResult)
        {
            await messageBroker.ReportJobAsync(jobResult);
        }

        internal static async Task RetryJobAsync(Schedule schedule)
        {
            await messageBroker.JobHandler.RetryJobAsync(schedule);
        }

        internal static async Task RunAfterUMJobAsync(Schedule schedule)
        {
            await messageBroker.PublishRunAfterUMJobAsync(schedule);
        }

        internal static async Task<UMInfoModel> GetUMInfoAsync()
        {
            return await messageBroker.GetUMInfoAsync();
        }

        internal static async Task InsertTaskLogAsync(TaskLog taskLog)
        {
            await messageBroker.PublishTaskLogAsync(taskLog);
        }

        internal static async Task UpdateSchedulesAsync(IList<Schedule> schedules)
        {
            await messageBroker.UpdateSchedulesAsync(schedules);
        }
    }
}