namespace Fanex.SimpleMailDelivery;

using MassTransit;
using MassTransit.DependencyInjection;
using Models;

public interface MailClient
{
    Task SendFunctionMailAsync(Mail mail);
}

public class MailClientImp(SettingModel settingModel, Bind<MailClientBus, IPublishEndpoint> publishEndpoint) : MailClient
{
    public async Task SendFunctionMailAsync(Mail mail)
    {
        mail.ApplicationSendMailId = mail.ApplicationSendMailId == 0
            ? settingModel.ApplicationSendMailId
            : mail.ApplicationSendMailId;

        mail.SiteId = settingModel.SiteId;

        await publishEndpoint.Value.Publish(mail);
    }
}