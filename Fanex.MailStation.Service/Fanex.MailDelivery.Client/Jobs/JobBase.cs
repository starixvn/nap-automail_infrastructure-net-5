using Fanex.MailDelivery.Client.Logging;
using Fanex.MailDelivery.Core;
using Fanex.MailDelivery.Core.Messaging;
using Fanex.MailDelivery.Core.Models;
using Quartz;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Fanex.MailDelivery.Client.Jobs;

public abstract class JobBase : IJob
{
    private StringBuilder logger;
    private const int ServerGMT = -4;

    public async Task Execute(IJobExecutionContext context)
    {
        Schedule schedule = null;
        logger = new StringBuilder($"Client Logs:{Environment.NewLine}");

        try
        {
            schedule = context.MergedJobDataMap[MailDeliveryConstants.ScheduleJobDataKey] as Schedule;

            if (schedule != null)
            {
                await StartGenerateMail(context, schedule.Clone());
            }
        }
        catch (Exception ex)
        {
            LogService.LogError(schedule, ex);
        }
    }

    private static async Task InsertTaskLogAsync(int taskId, JobStatus status, string message)
    {
        if (taskId > 0)
        {
            var taskLog = new TaskLog
            {
                LogTime = DateTime.Now,
                Message = message,
                Status = (int)status,
                TaskId = taskId
            };

            await MailClientHandler.InsertTaskLogAsync(taskLog);
        }
    }

    private async Task StartGenerateMail(IJobExecutionContext context, Schedule schedule)
    {
        string message;
        var umInfo = await MailClientHandler.GetUMInfoAsync();

        if (umInfo != null && umInfo.IsUnderMaintenanceTime)
        {
            var from = umInfo.From.AddHours(ServerGMT);
            var to = umInfo.To.AddHours(ServerGMT);
            schedule.CurrentFireTime = context.ScheduledFireTimeUtc.GetValueOrDefault().DateTime.ToLocalTime();
            message = $"Not allow to run the scheduled mail #{schedule.Id} ({schedule.Name}) in UM time ({from} - {to}) GMT{ServerGMT}. " +
                      $"It'll run after UM.";

            logger.AppendLine($"> {message}");
            LogService.LogInfo(message);

            await MailClientHandler.RunAfterUMJobAsync(schedule);
        }
        else
        {
            var remoteSchedule = await MailClientHandler.GetScheduleAsync(schedule.Id);

            if (remoteSchedule == null)
            {
                throw new InvalidOperationException($"Scheduled Mail #{schedule.Id} is not found.");
            }

            if (!remoteSchedule.Active || !schedule.Active)
            {
                message = $"Scheduled Mail #{schedule.Id} ({schedule.Name}) is disabled => Cannot run.";

                LogService.LogInfo(message);

                await InsertTaskLogAsync(schedule.TaskId, JobStatus.JobFailed, message);
            }
            else
            {
                var canRun = await CanRunAsync(schedule);

                if (canRun)
                {
                    await GenerateMailAsync(context, schedule);
                }
            }
        }
    }

    private async Task<bool> CanRunAsync(Schedule schedule)
    {
        var canRun = true;

        if (schedule.AutoRetry && schedule.TaskId > 0)
        {
            var mailTask = await MailClientHandler.RequestMailTaskByIdAsync(new GetMailTaskByIdRequest { TaskId = schedule.TaskId });

            if (mailTask != null)
            {
                canRun = mailTask.Status != (int)JobStatus.Success
                         && mailTask.Status != (int)JobStatus.SuccessWithoutSendmail
                         && mailTask.Status != (int)JobStatus.Running;

                if (!canRun)
                {
                    var message = $"The task status of the mail #{schedule.Id} ({schedule.Name}) is {((JobStatus)mailTask.Status).ToString()} => Stop auto-retry.";

                    LogService.LogInfo(message);

                    await InsertTaskLogAsync(schedule.TaskId, (JobStatus)mailTask.Status, message);
                }
            }
        }

        return canRun;
    }

    private async Task GenerateMailAsync(IJobExecutionContext context, Schedule schedule)
    {
        var message = $"Scheduled Mail #{schedule.Id} ({schedule.Name}) is running...";
        logger.AppendLine($"> {message}");
        LogService.LogInfo(message);

        var jobResult = new JobMessage
        {
            JobStatus = JobStatus.Running,
            ScheduledTime = schedule.CurrentFireTime ?? context.ScheduledFireTimeUtc.GetValueOrDefault().DateTime.ToLocalTime(),
            RunningTime = context.FireTimeUtc.DateTime.ToLocalTime(),
            Schedule = schedule.Clone()
        };

        if (schedule.TaskId == 0)
        {
            message = $"Adding a running task for the mail #{schedule.Id} ({schedule.Name})";
            logger.AppendLine($"> {message}");
            LogService.LogInfo(message);

            jobResult = await MailClientHandler.RequestAddMailTaskForRunningJobAsync(jobResult);

            schedule.TaskId = jobResult.Schedule.TaskId;
        }

        if (schedule.TaskId == 0)
        {
            throw new InvalidOperationException($"Cannot add a running mail task for the scheduled mail #{schedule.Id} ({schedule.Name}).");
        }

        message = $"Running the task #{schedule.TaskId} of the mail #{schedule.Id} ({schedule.Name})";

        logger.AppendLine($"> {message}");
        LogService.LogInfo(message);

        var mailTask = await MailClientHandler.RequestMailTaskByIdAsync(new GetMailTaskByIdRequest
        {
            TaskId = schedule.TaskId
        });

        if (mailTask != null && mailTask.Status != (int)JobStatus.Abort)
        {
            jobResult = await HandleJob(context, schedule, jobResult);
        }
        else
        {
            jobResult.JobStatus = JobStatus.Abort;
            message = $"Cannot run job to generate mail #{schedule.Id} ({schedule.Name}) because mail task #{schedule.TaskId} is aborted by user";

            logger.AppendLine($"> {message}");
            LogService.LogInfo(message);
        }

        if (jobResult.TaskLog != null)
        {
            jobResult.TaskLog.Message = logger.ToString();
        }
        else
        {
            jobResult.TaskLog = new TaskLog
            {
                LogTime = DateTime.Now,
                Message = logger.ToString(),
                StackTrace = string.Empty
            };
        }

        await MailClientHandler.ReportJobAsync(jobResult);

        if (jobResult.JobStatus == JobStatus.Success || jobResult.JobStatus == JobStatus.SuccessWithoutSendmail)
        {
            schedule.TaskId = 0;
        }
    }

    private async Task<JobMessage> HandleJob(IJobExecutionContext context, Schedule schedule, JobMessage jobMessage)
    {
        var jobResult = jobMessage;
        var message = $"Running job to generate the mail #{schedule.Id} ({schedule.Name})";

        logger.AppendLine($"> {message}");
        LogService.LogInfo(message);

        await MailClientHandler.ReportJobAsync(jobResult);

        jobResult = RunJob(context, schedule, jobResult);

        jobResult.Schedule = schedule;

        if (jobResult.JobStatus == JobStatus.Success || jobResult.JobStatus == JobStatus.SuccessWithoutSendmail)
        {
            message = $"Generated the mail #{schedule.Id} ({schedule.Name}) successfully";

            logger.AppendLine($"> {message}");
            LogService.LogInfo(message);
        }
        else
        {
            message = $"The task status of the mail #{schedule.Id} ({schedule.Name}) is {jobResult.JobStatus.ToString()}";
            logger.AppendLine($"> {message}");
            LogService.LogInfo(message);

            await RetryJobAsync(jobResult.Schedule);
        }

        return jobResult;
    }

    private JobMessage RunJob(IJobExecutionContext context, Schedule schedule, JobMessage jobMessage)
    {
        JobMessage jobResult;

        try
        {
            var mailJobResult = Run(schedule, context);

            if (mailJobResult == null || mailJobResult.Mail == null)
            {
                throw new InvalidOperationException("Mail information cannot be null");
            }

            jobResult = mailJobResult;
            jobResult.Mail.MailingListId = schedule.Id;
        }
        catch (Exception ex)
        {
            LogService.LogError(schedule, ex);

            jobResult = jobMessage;
            jobResult.TaskLog = new TaskLog
            {
                LogTime = DateTime.Now,
                Message = ex.InnerException?.Message ?? ex.Message,
                StackTrace = ex.StackTrace
            };

            jobResult.JobStatus = JobStatus.JobFailed;
            logger.AppendLine($"> Generating mail failed -> {jobResult.TaskLog.Message}");
        }

        return jobResult;
    }

    protected abstract JobMessage Run(Schedule schedule, IJobExecutionContext jobContext);

    private async Task RetryJobAsync(Schedule schedule)
    {
        schedule.CurrentRetryTimes++;
        string message;
        var remoteSchedule = await MailClientHandler.GetScheduleAsync(schedule.Id);

        if (schedule.MaxRetryTimes == 0)
        {
            message = $"The mail #{schedule.Id} ({schedule.Name}) is not allowed to retry in its setting.";

            logger.AppendLine($"> {message}");
            LogService.LogInfo(message);
        }
        else if (remoteSchedule != null && !remoteSchedule.Active)
        {
            message = $"Scheduled Mail #{schedule.Id} ({schedule.Name}) is disabled => Cannot retry.";

            logger.AppendLine($"> {message}");
            LogService.LogInfo(message);
        }
        else if (schedule.CanRetry())
        {
            schedule.AutoRetry = true;
            message = $"Retry({schedule.CurrentRetryTimes}) to generate the mail #{schedule.Id} ({schedule.Name}) in {schedule.RetryInterval} minutes...";

            logger.AppendLine($"> {message}");
            LogService.LogInfo(message);

            await MailClientHandler.RetryJobAsync(schedule);
        }
        else
        {
            message = $"Exceeded maximum retrying time({schedule.MaxRetryTimes}) for the mail #{schedule.Id} ({schedule.Name})";

            logger.AppendLine($"> {message}");
            LogService.LogInfo(message);
        }
    }
}