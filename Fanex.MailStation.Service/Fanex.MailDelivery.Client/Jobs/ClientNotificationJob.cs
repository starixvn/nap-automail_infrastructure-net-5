using Quartz;
using System.Threading.Tasks;

namespace Fanex.MailDelivery.Client.Jobs;

public class ClientNotificationJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await MailClientHandler.PublishClientNotifyMessageAsync();
    }
}