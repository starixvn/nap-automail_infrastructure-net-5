using Quartz;
using System.Threading.Tasks;

namespace Fanex.MailDelivery.Client.Jobs
{
    public class UpdateScheduleJob : IJob
    {
        public const string JobIdentity = "UpdateScheduleJobName";
        public const string TriggerIdentity = "UpdateScheduleJobTriggerName";

        public async Task Execute(IJobExecutionContext context)
        {
            var schedules = await MailClientHandler.GetSchedulesAsync();

            await MailClientHandler.UpdateSchedulesAsync(schedules);
        }
    }
}
