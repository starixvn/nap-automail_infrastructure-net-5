using Fanex.MailDeliver.Core.Infrastructure.Services;
using Quartz;
using Quartz.Impl;
using IScheduler = Quartz.IScheduler;

namespace Fanex.MailDeliver.Service.Service
{
    public class MailResendService
    {
        public static async void Resend()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            IJobDetail job = JobBuilder.Create<MailResendJobImp>().Build();
            ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("MailResend", "MailResend")
            .StartNow()
            .WithCronSchedule(AppConfigService.Config.CronScheduleResendMail)
            .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}