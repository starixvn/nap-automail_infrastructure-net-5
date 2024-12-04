using Fanex.MailDelivery.Services.Services;
using Quartz;
using System.Threading.Tasks;

namespace Fanex.MailStation.Service.Jobs;

public class UpdateFailedForLongRunningTaskJob(MailService mailService) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        mailService.UpdateFailedForLongRunningMailTask();

        return Task.CompletedTask;
    }
}