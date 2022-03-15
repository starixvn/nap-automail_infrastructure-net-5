using EasyNetQ;
using Fanex.Data;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.MailDeliver.Business;
using Fanex.MailDeliver.Business.Implement;
using Fanex.MailDeliver.Business.Interface;
using Fanex.MailDeliver.Business.Worker;
using Fanex.MailDeliver.Core.Business;
using Fanex.MailDeliver.Core.Business.Implement;
using Fanex.MailDeliver.Core.Business.Worker;
using Fanex.MailDeliver.Core.Infrastructure.Models;
using Fanex.MailDeliver.Core.Infrastructure.Services;
using Fanex.MailDeliver.DataAccess.Implement;
using Fanex.MailDeliver.DataAccess.Interface;
using Fanex.MailDeliver.Model;
using Fanex.MailDeliver.Service.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using SharpRaven;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Fanex.MailDeliver.Service
{
    public static class ServiceCollectionExtensions
    {
        public static void LoadConfig(this IServiceCollection services)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var text = File.ReadAllText("appsettings.json");
            AppConfigService.Config = JsonConvert.DeserializeObject<ConfigSettingModel>(text);
        }

        public static void RegisterSentry(this IServiceCollection services)
        {
            var sentry = new SentryLogging(new SentryEngineOptions
            {
                Dsn = new Dsn(AppConfigService.Config.SentryClientKey)
            });

            LogManager
               .SetDefaultLogCategory(AppConfigService.Config.CategorySentryLog)
               .Use(new List<ILogEngine>
               {
                    sentry
               });
        }

        public static void RegisterFanexData(this IServiceCollection services, IConfiguration Configuration)
        {
            var connections = Configuration.GetSection("ConnectionStrings")
                .GetChildren()
                .AsEnumerable()
                .ToDictionary
                (
                     x => x.Key,
                     y => y.Value
                );

            DbSettingProviderManager
             .StartNewSession()
             .UseConnectionStrings(connections)
             .UseDefaultDbSettingProvider(Configuration.GetSection("StoredProcedurePath").Value)
             .Run();
        }

        public static void RegisterInternalServices(this IServiceCollection services)
        {
            services.AddSingleton<IDynamicRepository, DynamicRepository>();
            services.AddSingleton<IMailService, MailService>();
            services.AddSingleton<SendMailWorker, SendMailWorkerImp>();
            services.AddSingleton<ResendMailWorker, ResendMailWorkerImp>();
            services.AddSingleton<IMailDeliverBusiness, MailDeliverBusiness>();
            services.AddSingleton<DataService, DataServiceImp>();
            services.AddSingleton<MailExtension, MailExtensionImp>();
            services.AddSingleton<IMailDeliverDataAccess, MailDeliverDataAccess>();
            services.AddSingleton<MailKitService, MailKitServiceImp>();
            services.AddSingleton<MailResendJob>();

            services.AddSingleton<IBus>(RabbitHutch.CreateBus(AppConfigService.Config.RabbitMQConnectionString));
        }

        public static void SubcriberMailService(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            AppConfigService.Bus = serviceProvider.GetRequiredService<IBus>();
            var isConnected = AppConfigService.Bus.IsConnected;
            var count = 10;
            while (!isConnected && count > 0)
            {
                Thread.Sleep(1000);
                AppConfigService.Bus = RabbitHutch.CreateBus(AppConfigService.Config.RabbitMQConnectionString);
                isConnected = AppConfigService.Bus.IsConnected;
                count--;
            }

            AppConfigService.Bus.Subscribe<MailRecord>("MailRecord", mail => HandlerMailRecord(mail, serviceProvider));
        }

        private static void HandlerMailRecord(MailRecord mailRecord, IServiceProvider serviceProvider)
        {
            var sendMailWorker = serviceProvider.GetRequiredService<SendMailWorker>();
            sendMailWorker.Run(mailRecord);
        }

        public static void StartScheduler(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
            scheduler.JobFactory = new MailResendFactory(serviceProvider);

            var job = JobBuilder.Create<MailResendJob>().Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity("MailResend", "MailResend")
                .StartNow()
                .WithCronSchedule(AppConfigService.Config.CronScheduleResendMail)
                .Build();

            scheduler.ScheduleJob(job, trigger).Wait();
            scheduler.Start().Wait();
        }
    }
}