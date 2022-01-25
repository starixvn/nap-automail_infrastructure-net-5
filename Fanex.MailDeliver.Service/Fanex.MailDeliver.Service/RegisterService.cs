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
using SharpRaven;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fanex.MailDeliver.Service
{
    public static class RegisterService
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public static void RegisterServices(IConfiguration configuration, IServiceCollection services)
        {
            LoadConfig();
            SentryRegister();
            ConfigureFanexData(configuration);
            DependencyInjection(services);
            SubcriberMailService();
            MailResendService.Resend();
        }

        private static void LoadConfig()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var text = File.ReadAllText("appsettings.json");
            AppConfigService.Config = JsonConvert.DeserializeObject<ConfigSettingModel>(text);
        }

        private static void SentryRegister()
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

        private static void ConfigureFanexData(IConfiguration Configuration)
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

        private static void DependencyInjection(IServiceCollection services)
        {
            services.AddSingleton<IDynamicRepository, DynamicRepository>();
            services.AddSingleton<IMailService, MailService>();
            services.AddSingleton<SendMailWorker, SendMailWorkerImp>();
            services.AddSingleton<ResendMailWorker, ResendMailWorkerImp>();
            services.AddSingleton<IMailDeliverBusiness, MailDeliverBusiness>();
            services.AddSingleton<DataService, DataServiceImp>();
            services.AddSingleton<MailExtension, MailExtensionImp>();
            services.AddSingleton<IMailDeliverDataAccess, MailDeliverDataAccess>();

            services.AddSingleton<IBus>(RabbitHutch.CreateBus(AppConfigService.Config.RabbitMQConnectionString));

            ServiceProvider = services.BuildServiceProvider();
        }

        private static void SubcriberMailService()
        {
            AppConfigService.Bus = ServiceProvider.GetRequiredService<IBus>();
            AppConfigService.Bus.Subscribe<MailRecord>("MailRecord", mail => HandlerMailRecord(mail));
        }

        private static void HandlerMailRecord(MailRecord mailRecord)
        {
            var sendMailWorker = ServiceProvider.GetRequiredService<SendMailWorker>();
            sendMailWorker.Run(mailRecord);
        }
    }
}