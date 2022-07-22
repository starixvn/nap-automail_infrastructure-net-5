using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;

namespace Fanex.MailStation.Service.Jobs
{
    public class UpdateFailedForLongRunningTaskJobFactory : IJobFactory
    {
        private readonly IServiceProvider serviceProvider;

        public UpdateFailedForLongRunningTaskJobFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return serviceProvider.GetService<UpdateFailedForLongRunningTaskJob>();
        }

        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}