using EasyNetQ;
using Fanex.Caching;
using Fanex.MailDelivery.Core;
using Fanex.MailDelivery.Core.Models;
using Fanex.MailDelivery.Services.Repositories;
using Fanex.MailDelivery.Services.Services;
using Fanex.MailStation.Service.Consumers;
using Fanex.MailStation.Service.Jobs;
using Fanex.MailStation.Service.MessageBrokers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using System;
using System.IO;
using MailServiceAppSettings = Fanex.MailDeliver.MailClient.AppSettings;

namespace Fanex.MailStation.Service;

using MassTransit;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

public static class ServiceCollectionExtensions
{
    public static void AddConfiguration(this IServiceCollection services)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        var text = File.ReadAllText("appsettings.json");
        AppConfigService.AppConfig = JsonConvert.DeserializeObject<ConfigSettingModel>(text);
        MailServiceAppSettings.AppMailId = AppConfigService.AppConfig!.AppMailId;
        MailServiceAppSettings.AppMailAddress = AppConfigService.AppConfig.AppMailAddress;
    }

    public static void AddInternalService(this IServiceCollection services)
    {
        services.AddSingleton<UpdateFailedForLongRunningTaskJob>();
        services.AddSingleton<MessageBroker, MessageBrokerImpl>();
        services.AddSingleton<MailService, MailServiceImpl>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<LogService, LogServiceImpl>();
        services.AddSingleton<UMService, UMServiceImpl>();
        services.AddSingleton<IMailRepository, MailRepository>();
        services.AddSingleton(RabbitHutch.CreateBus(AppConfigService.AppConfig.RabbitMQConnectionString));
    }

    public static void AddMessageBroker(this IServiceCollection services)
    {
        var messageBroker = services.BuildServiceProvider().GetRequiredService<MessageBroker>();
        messageBroker.Start();
    }

    public static void AddMassTransitForMail(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetValue("RabbitMQConnectionString", "");
        var map = connectionString.Split(';')
            .Select(p => p.Split('='))
            .ToDictionary(p => p[0].ToLower(), p => p[1]);

        var host = map.GetValueOrDefault("host", string.Empty).Replace(":5672", string.Empty);
        var virtualHost = map.GetValueOrDefault("virtualhost", string.Empty);
        var username = map.GetValueOrDefault("username", string.Empty);
        var password = map.GetValueOrDefault("password", string.Empty);
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<SimpleMailDeliveryConsumer, SimpleMailDeliveryConsumerDefinition>();
            busConfigurator.UsingRabbitMq((context, busFactoryConfigurator) =>
            {
                busFactoryConfigurator.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });
                busFactoryConfigurator.ConfigureEndpoints(context);
            });
        });
    }

    public static void AddScheduler(this IServiceCollection services)
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