using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;

namespace Fanex.MailStation.Service.Jobs;

public class UpdateFailedForLongRunningTaskJobFactory(IServiceProvider serviceProvider) : IJobFactory
{
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