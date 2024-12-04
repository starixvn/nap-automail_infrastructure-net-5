using Fanex.MailDelivery.Client.Configurations;
using Fanex.MailDelivery.Client.Jobs;
using Fanex.MailDelivery.Client.Logging;
using Fanex.MailDelivery.Core;
using Fanex.MailDelivery.Core.Models;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.MailDelivery.Client.Handlers;

public interface IJobHandler
{
    Task StartAsync();

    Task StopAsync();

    Task UpdateScheduleAsync(Schedule schedule);

    Task RetryJobAsync(Schedule schedule);

    Task RetryAllProcessAsync(Schedule schedule);

    Task InitializeSchedulerAsync();
}

public class JobHandler(ISchedulerFactory schedulerFactory, MailStationSetting mailStationSetting)
    : IJobHandler
{
    private IScheduler scheduler;
    private readonly ISchedulerFactory schedulerFactory = schedulerFactory ?? new StdSchedulerFactory();

    public JobHandler(MailStationSetting mailStationSetting) : this(null, mailStationSetting)
    {
    }

    public async Task InitializeSchedulerAsync()
    {
        if (mailStationSetting.HasSchedule())
        {
            scheduler = await schedulerFactory.GetScheduler();
            await scheduler.Shutdown();

            scheduler = await schedulerFactory.GetScheduler();
            await scheduler.Start();
        }
    }

    public async Task StartAsync()
    {
        if (mailStationSetting.HasSchedule())
        {
            await StartUpdateActiveTimeJob();

            var schedules = await MailClientHandler.GetSchedulesAsync();

            await StartJobsAsync(schedules);

            await StartUpdateSchedulesJob();
        }
    }

    public async Task StopAsync()
    {
        if (scheduler != null)
        {
            await scheduler.Shutdown();
        }
    }

    public async Task RetryAllProcessAsync(Schedule schedule)
    {
        if (mailStationSetting.HasScheduleId(schedule.Id))
        {
            await StopAutoRetry(schedule);

            IJobDetail job = GetRetryJob(schedule);
            ITrigger trigger = GetRetryAllProcessTrigger(schedule);

            await scheduler.ScheduleJob(job, trigger);
        }
    }

    private async Task StopAutoRetry(Schedule schedule)
    {
        if (schedule.TaskId > 0)
        {
            var autoRetryTriggerKey = new TriggerKey(schedule.TaskId.ToString(), MailDeliveryConstants.MailCentralizedTriggerGroup);
            var oldTrigger = await scheduler.GetTrigger(autoRetryTriggerKey);

            if (oldTrigger != null)
            {
                await scheduler.UnscheduleJob(autoRetryTriggerKey);
            }
        }
    }

    public async Task RetryJobAsync(Schedule schedule)
    {
        if (schedule.TaskId > 0)
        {
            await StopAutoRetry(schedule);

            IJobDetail job = GetRetryJob(schedule);
            ITrigger trigger = GetAutoRetryTrigger(schedule);

            await scheduler.ScheduleJob(job, trigger);
        }
    }

    public async Task UpdateScheduleAsync(Schedule schedule)
    {
        // If the schedule is belonged to current Client.
        // We just manage the Schedule belong to us (in app settings).
        if (mailStationSetting.HasScheduleId(schedule.Id))
        {
            var triggerKey = new TriggerKey(schedule.Id.ToString(), MailDeliveryConstants.MailCentralizedTriggerGroup);
            var oldTrigger = await scheduler.GetTrigger(triggerKey);

            if (oldTrigger != null)
            {
                var oldSchedule = oldTrigger.JobDataMap[MailDeliveryConstants.ScheduleJobDataKey] as Schedule;
                bool hasChange = oldSchedule == null || !oldSchedule.RepeatExpression.Equals(schedule.RepeatExpression, StringComparison.OrdinalIgnoreCase);

                if (!schedule.Active)
                {
                    LogService.LogInfo($"Mail #{schedule.Id} ({schedule.Name}) is disabled");

                    await scheduler.UnscheduleJob(triggerKey);
                }
                else if (schedule.IsScheduleEnd())
                {
                    LogService.LogInfo($"Mail #{schedule.Id} ({schedule.Name}) is expired");

                    await scheduler.UnscheduleJob(triggerKey);
                }
                else if (hasChange)
                {
                    LogService.LogInfo($"Mail #{schedule.Id} ({schedule.Name}) is updated the new schedule ({schedule.RepeatExpression})");

                    ITrigger newTrigger = GetTrigger(schedule);

                    await scheduler.RescheduleJob(triggerKey, newTrigger);
                }
            }
            else
            {
                if (schedule.Active && !schedule.IsScheduleEnd())
                {
                    LogService.LogInfo($"Mail #{schedule.Id} ({schedule.Name}) is enabled and triggered at the schedule ({schedule.RepeatExpression})");

                    IJobDetail job = GetJobDetail(schedule);
                    ITrigger trigger = GetTrigger(schedule);

                    await scheduler.ScheduleJob(job, trigger);
                }
            }
        }
    }

    public async Task StartJobsAsync(IList<Schedule> schedules)
    {
        if (schedules.Count == 0)
        {
            LogService.LogInfo("There is no any schedules to start.");
        }

        foreach (var schedule in schedules)
        {
            if (schedule != null && schedule.Active && !schedule.IsScheduleEnd())
            {
                if (schedule.SiteId != mailStationSetting.SiteId)
                {
                    var message = $"Only run for mails belonging the same site #{mailStationSetting.SiteId}. " +
                                  $"But mail #{schedule.Id} ({schedule.Name}) belongs another site #{schedule.SiteId}, it's ignored.";

                    LogService.LogError(message);
                }
                else
                {
                    LogService.LogInfo($"Mail #{schedule.Id} ({schedule.Name}) is triggered by the schedule ({schedule.RepeatExpression})");

                    IJobDetail job = GetJobDetail(schedule);
                    ITrigger trigger = GetTrigger(schedule);

                    // Unscheduled all redundant job.
                    await scheduler.UnscheduleJob(trigger.Key);

                    await scheduler.ScheduleJob(job, trigger);
                }
            }
        }
    }

    public async Task StartUpdateActiveTimeJob()
    {
        IJobDetail job = JobBuilder.Create<ClientNotificationJob>()
            .WithIdentity(MailDeliveryConstants.ClientNotificationJobName)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(MailDeliveryConstants.ClientNotificationTriggerName)
            .StartNow()
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(MailDeliveryConstants.ClientNotificationIntervalTimeInSecond).RepeatForever())
            .ForJob(MailDeliveryConstants.ClientNotificationJobName)
            .Build();

        await scheduler.DeleteJob(job.Key);

        await scheduler.ScheduleJob(job, trigger);
    }

    public async Task StartUpdateSchedulesJob()
    {
        IJobDetail job = JobBuilder.Create<UpdateScheduleJob>()
            .WithIdentity(UpdateScheduleJob.JobIdentity)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(UpdateScheduleJob.TriggerIdentity)
            .StartNow()
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(mailStationSetting.UpdateSchedulesIntervalInSeconds).RepeatForever())
            .ForJob(UpdateScheduleJob.JobIdentity)
            .Build();

        await scheduler.DeleteJob(job.Key);

        await scheduler.ScheduleJob(job, trigger);
    }

    private IJobDetail GetJobDetail(Schedule schedule)
    {
        string jobName = string.Format(MailDeliveryConstants.AutoMailJobNameFormat, schedule.Id);

        // Define the job and tie it to our JobRequest class
        Type jobType = mailStationSetting.GetJobType(schedule.Id);
        IJobDetail jobDetail = JobBuilder.Create(jobType)
            .WithIdentity(jobName, MailDeliveryConstants.MailCentralizedTriggerGroup)
            .Build();

        return jobDetail;
    }

    private JobDataMap GetJobDataMap(Schedule schedule)
    {
        JobDataMap jobDataMap = new JobDataMap();
        jobDataMap.Add(MailDeliveryConstants.ScheduleJobDataKey, schedule);

        var schedules = mailStationSetting.Schedules;

        foreach (ScheduleSetting scheduleConfig in schedules)
        {
            if (schedule.Id == scheduleConfig.Id)
            {
                foreach (JobInputDataSetting jobData in scheduleConfig.JobInputData)
                {
                    jobDataMap.Add(jobData.Name, jobData.Value);
                }
            }
        }

        return jobDataMap;
    }

    private IJobDetail GetRetryJob(Schedule schedule)
    {
        schedule.ConvertToServerTime(mailStationSetting.IsDevEnvironment());

        string jobName = string.Format(MailDeliveryConstants.AutoMailJobNameFormat, schedule.Id + Guid.NewGuid().ToString());

        // Define the job and tie it to our JobRequest class
        Type jobType = mailStationSetting.GetJobType(schedule.Id);

        IJobDetail job = JobBuilder.Create(jobType)
            .WithIdentity(jobName, MailDeliveryConstants.MailCentralizedTriggerGroup)
            .Build();

        return job;
    }

    private ITrigger GetRetryAllProcessTrigger(Schedule schedule)
    {
        JobDataMap jobDataMap = GetJobDataMap(schedule);

        ITrigger retryTrigger = TriggerBuilder.Create()
            .StartNow()
            .UsingJobData(jobDataMap)
            .Build();

        return retryTrigger;
    }

    private ITrigger GetAutoRetryTrigger(Schedule schedule)
    {
        schedule.ConvertToServerTime(mailStationSetting.IsDevEnvironment());
        JobDataMap jobDataMap = GetJobDataMap(schedule);

        ITrigger retryTrigger = TriggerBuilder.Create()
            .WithIdentity(schedule.TaskId.ToString(), MailDeliveryConstants.MailCentralizedTriggerGroup)
            .StartAt(DateTimeOffset.Now.AddMinutes(schedule.RetryInterval))
            .UsingJobData(jobDataMap)
            .Build();

        return retryTrigger;
    }

    private ITrigger GetTrigger(Schedule schedule)
    {
        schedule.ConvertToServerTime(mailStationSetting.IsDevEnvironment());

        JobDataMap jobData = GetJobDataMap(schedule);

        TriggerBuilder triggerBuilder = TriggerBuilder.Create();

        // Start Schedule
        if (IsScheduleStartNow(schedule))
        {
            triggerBuilder.StartNow();
        }
        else
        {
            triggerBuilder.StartAt(schedule.StartTime);
        }

        // End Schedule
        if (schedule.EndTime != null)
        {
            triggerBuilder.EndAt(new DateTimeOffset((DateTime)schedule.EndTime));
        }

        ITrigger trigger = triggerBuilder
            .WithIdentity(schedule.Id.ToString(), MailDeliveryConstants.MailCentralizedTriggerGroup)
            .WithCronSchedule(schedule.RepeatExpression)
            .UsingJobData(jobData)
            .Build();

        return trigger;
    }

    private static bool IsScheduleStartNow(Schedule schedule)
    {
        DateTimeOffset startTimeOffSet = new DateTimeOffset(schedule.StartTime);
        DateTimeOffset currentTimeOffSet = new DateTimeOffset(DateTime.Now);
        TimeSpan timeGap = startTimeOffSet - currentTimeOffSet;

        return timeGap.TotalMilliseconds <= 0;
    }
}