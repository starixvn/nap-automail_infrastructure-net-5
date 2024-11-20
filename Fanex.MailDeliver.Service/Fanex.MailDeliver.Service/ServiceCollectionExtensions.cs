using EasyNetQ;
using Fanex.Data;
using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.MailDeliver.Core.Business;
using Fanex.MailDeliver.Core.Business.Implement;
using Fanex.MailDeliver.Core.Business.Interface;
using Fanex.MailDeliver.Core.Business.Worker;
using Fanex.MailDeliver.Core.DataAccess.Implement;
using Fanex.MailDeliver.Core.DataAccess.Interface;
using Fanex.MailDeliver.Core.Infrastructure.Models;
using Fanex.MailDeliver.Core.Infrastructure.Services;
using Fanex.MailDeliver.MailClient.Model;
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

namespace Fanex.MailDeliver.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Services { get; set; }

        public static void LoadConfiguration(this IServiceCollection services)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var text = File.ReadAllText("appsettings.json");
            AppConfigService.Config = JsonConvert.DeserializeObject<ConfigSettingModel>(text);
        }

        public static void SentryRegister(this IServiceCollection services)
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

        public static void FanexDataRegister(this IServiceCollection services, IConfiguration config)
        {
            try
            {
                DbSettingProviderManager.StartNewSession()
                    .WithSqlServer(config)
                    .UseDefaultDbSettingProvider("App_Data", true)
                    .Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void InternalServiceRegister(this IServiceCollection services)
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
            AppConfigService.Bus.PubSub.Subscribe<MailRecord>("MailRecord", mail => HandlerMailRecord(mail, serviceProvider));
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