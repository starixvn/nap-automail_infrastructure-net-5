using Fanex.MailDelivery.Services.Services;
using Quartz;
using System.Threading.Tasks;

namespace Fanex.MailStation.Service.Jobs
{
    public class UpdateFailedForLongRunningTaskJob : IJob
    {
        private readonly MailService mailService;

        public UpdateFailedForLongRunningTaskJob(MailService mailService)
        {
            this.mailService = mailService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            mailService.UpdateFailedForLongRunningMailTask();

            return Task.CompletedTask;
        }
    }
}