using EasyNetQ;
using Fanex.Data;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.MailDelivery.Core;
using Fanex.MailDelivery.Services.Repositories;
using Fanex.MailDelivery.Services.Services;
using Fanex.MailStation.Service.Jobs;
using Fanex.MailStation.Service.MessageBrokers;
using Fanex.MailStation.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using SharpRaven;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.MailStation.Service
{
    public static class ServiceCollectionExtensions
    {
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

        public static void FanexDataRegister(this IServiceCollection services, IConfiguration Configuration)
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