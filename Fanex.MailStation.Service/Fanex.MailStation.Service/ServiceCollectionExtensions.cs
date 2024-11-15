using EasyNetQ;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.MailDelivery.Core;
using Fanex.MailDelivery.Core.Models;
using Fanex.MailDelivery.Services.Repositories;
using Fanex.MailDelivery.Services.Services;
using Fanex.MailStation.Service.Jobs;
using Fanex.MailStation.Service.MessageBrokers;
using Fanex.MailStation.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using SharpRaven;
using System;
using System.Collections.Generic;
using System.IO;
using MailServiceAppSettings = Fanex.MailDeliver.MailClient.AppSettings;

namespace Fanex.MailStation.Service
{
    public static class ServiceCollectionExtensions
    {
        public static void LoadConfiguration(this IServiceCollection services)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var text = File.ReadAllText("appsettings.json");
            AppConfigService.AppConfig = JsonConvert.DeserializeObject<ConfigSettingModel>(text);
            MailServiceAppSettings.AppMailId = AppConfigService.AppConfig!.AppMailId;
            MailServiceAppSettings.AppMailAddress = AppConfigService.AppConfig.AppMailAddress;
        }

        public static void SentryRegister(this IServiceCollection services)
        {
            var sentry = new SentryLogging(new SentryEngineOptions
            {
                Dsn = new Dsn(AppConfigService.AppConfig.SentryClientKey)
            });

            LogManager
                .SetDefaultLogCategory(AppConfigService.AppConfig.CategorySentryLog)
                .Use(new List<ILogEngine>
                {
                    sentry
                });
        }

        public static void InternalServiceRegister(this IServiceCollection services)
        {
            services.AddSingleton<UpdateFailedForLongRunningTaskJob>();
            services.AddSingleton<MessageBroker, MessageBrokerImpl>();
            services.AddSingleton<MailService, MailServiceImpl>();
            services.AddSingleton<MailDelivery.Services.Services.LogService, MailDelivery.Services.Services.LogServiceImpl>();
            services.AddSingleton<UMService, UMServiceImpl>();
            services.AddSingleton<IMailRepository, MailRepository>();
            services.AddSingleton<IBus>(RabbitHutch.CreateBus(AppConfigService.AppConfig.RabbitMQConnectionString));

            OnebookUMInitializer.InitializeUMClientManager();
            OnebookUMInitializer.RegisterSimpleInjector(services);
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

        public static void StartMessageBroker(this IServiceCollection services)
        {
            var messageBroker = services.BuildServiceProvider().GetRequiredService<MessageBroker>();
            messageBroker.Start();
        }

        public static void StartScheduler(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
            scheduler.JobFactory = new UpdateFailedForLongRunningTaskJobFactory(serviceProvider);

            var trigger = TriggerBuilder.Create()
            .WithIdentity("Fanex.MailStation.Service", "Fanex.MailStation.Service")
            .StartNow()
            .WithSimpleSchedule(x => x.WithIntervalInMinutes(AppConfigService.AppConfig.UpdateFailedForLongRunningTaskIntervalInMinutes).RepeatForever())
            .Build();

            var job = JobBuilder.Create<UpdateFailedForLongRunningTaskJob>().Build();
            scheduler.ScheduleJob(job, trigger).Wait();
            scheduler.Start().Wait();
        }
    }
}