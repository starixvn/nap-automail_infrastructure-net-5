using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;

namespace Fanex.MailDeliver.Service.Service
{
    public class MailResendFactory : IJobFactory
    {
        private readonly IServiceProvider serviceProvider;

        public MailResendFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return serviceProvider.GetService<MailResendJob>();
        }

        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}