namespace Fanex.SimpleMailDelivery;

using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMailDelivery(this IServiceCollection services, IConfiguration configuration)
    {
        var settingModel = ReadSettingModel(configuration);

        services.AddSingleton(settingModel);
        services.AddTransient<MailClient, MailClientImp>();

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.UsingRabbitMq((context, busFactoryConfigurator) =>
            {
                busFactoryConfigurator.Host(settingModel.Hosting, settingModel.VirtualHost,h =>
                {
                    h.Username(settingModel.Username ?? string.Empty);
                    h.Password(settingModel.Password ?? string.Empty);
                });
                busFactoryConfigurator.Publish<Mail>(x=>x.ExchangeType="topic");
                busFactoryConfigurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static SettingModel ReadSettingModel(IConfiguration configuration)
    {
        return new SettingModel
        {
            ApplicationSendMailId = ReadInt(configuration, "Fanex.SimpleMailDelivery:ApplicationSendMailId"),
            SiteId = ReadInt(configuration, "Fanex.SimpleMailDelivery:SiteId"),
            Hosting = configuration.GetSection("Fanex.SimpleMailDelivery:RabbitMQ:Hosting").Value,
            VirtualHost = configuration.GetSection("Fanex.SimpleMailDelivery:RabbitMQ:VirtualHost").Value,
            Username = configuration.GetSection("Fanex.SimpleMailDelivery:RabbitMQ:Username").Value,
            Password = configuration.GetSection("Fanex.SimpleMailDelivery:RabbitMQ:Password").Value,
        };
    }

    private static int ReadInt(IConfiguration configuration, string key)
    {
        var value = configuration.GetSection(key).Value ?? "0";
        
        return Convert.ToInt32(value);
    }
}