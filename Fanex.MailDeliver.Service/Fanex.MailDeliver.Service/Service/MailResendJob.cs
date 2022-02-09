using Fanex.MailDeliver.Core.Business.Worker;
using Quartz;
using System.Threading.Tasks;

namespace Fanex.MailDeliver.Service.Service
{
    public class MailResendJob : IJob
    {
        private readonly ResendMailWorker resendMailWorker;

        public MailResendJob(ResendMailWorker resendMailWorker)
        {
            this.resendMailWorker = resendMailWorker;
        }

        public Task Execute(IJobExecutionContext context)
        {
            resendMailWorker.Run();
            return Task.CompletedTask;
        }
    }
}