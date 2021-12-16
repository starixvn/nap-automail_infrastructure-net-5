using Fanex.MailDeliver.Core.Business.Worker;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Threading.Tasks;

namespace Fanex.MailDeliver.Service.Service
{
    public class MailResendJobImp : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var service = RegisterService.ServiceProvider.GetRequiredService<ResendMailWorker>();
            service.Run();
            return Task.CompletedTask;
        }
    }
}