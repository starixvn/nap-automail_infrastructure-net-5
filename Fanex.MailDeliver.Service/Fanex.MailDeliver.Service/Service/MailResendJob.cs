using Fanex.MailDeliver.Core.Business.Worker;
using Quartz;
using System.Threading.Tasks;

namespace Fanex.MailDeliver.Service.Service;

public class MailResendJob(ResendMailWorker resendMailWorker) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        resendMailWorker.Run();
        return Task.CompletedTask;
    }
}