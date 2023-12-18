using Fanex.MailDeliver.MailClient.Model;
using Fanex.MailDelivery.Core;
using Fanex.MailDelivery.Core.Messaging;
using Fanex.MailDelivery.Core.Models;
using Fanex.MailDelivery.Services.Criteria;
using Fanex.MailDelivery.Services.Criteria.Job;
using Fanex.MailDelivery.Services.Models;
using Fanex.MailDelivery.Services.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fanex.MailDelivery.Services.Services
{
    public interface MailService
    {
        Task UpdateJobStatus(JobMessage jobMessage);

        Task SendMail(Mail mail, int mailTaskId = 0);

        int AddMailTaskForRunningJob(JobMessage jobMessage);

        MailTaskModel GetMailTaskById(GetMailTaskByIdRequest request);

        GetScheduleListResponse GetScheduleList(GetScheduleListRequest request);

        void UpdateClientActiveTime(UpdateActiveTimeRequest updateActiveTimeRequest);

        void UpdateFailedForLongRunningMailTask();

        void InsertTaskLog(TaskLog log);
    }

    public class MailServiceImpl : MailService
    {
        private readonly IMailRepository mailRepository;
        private readonly LogService logService;
        private readonly int sendMailRetryLimit;
        private readonly int sendMailRetryIntervalInMinutes;

        public MailServiceImpl(
            LogService logService,
            IMailRepository mailRepository)
        {
            this.sendMailRetryIntervalInMinutes = AppConfigService.AppConfig.SendMailRetryIntervalInMinutes;
            this.sendMailRetryLimit = AppConfigService.AppConfig.SendMailRetryLimit;
            this.logService = logService;
            this.mailRepository = mailRepository;
        }

        public GetScheduleListResponse GetScheduleList(GetScheduleListRequest request)
        {
            var schedules = new List<Schedule>();

            foreach (int mail in request.MailIds)
            {
                try
                {
                    Schedule schedule = mailRepository.GetScheduleByMailId(new GetScheduleByMailIdCriteria
                    {
                        MailId = mail
                    });

                    if (schedule != null)
                    {
                        schedules.Add(schedule);
                    }
                }
                catch (Exception ex)
                {
                    logService.LogError(ex, mail);
                }
            }

            return new GetScheduleListResponse { Schedules = schedules };
        }

        public void UpdateClientActiveTime(UpdateActiveTimeRequest updateActiveTimeRequest)
        {
            try
            {
                var criteria = new UpdateClientActiveTimeCriteria
                {
                    MailIds = updateActiveTimeRequest.MailIds
                };

                mailRepository.UpdateClientActiveTime(criteria);
            }
            catch (Exception ex)
            {
                logService.LogError(ex);
            }
        }

        public async Task UpdateJobStatus(JobMessage jobMessage)
        {
            try
            {
                if (jobMessage.TaskLog != null)
                {
                    mailRepository.AddMailLog(new AddMailLogCriteria
                    {
                        LogTime = jobMessage.TaskLog.LogTime,
                        ExceptionMessage = jobMessage.TaskLog.Message,
                        StackTrace = jobMessage.TaskLog.StackTrace,
                        Status = (int)jobMessage.JobStatus,
                        TaskId = jobMessage.Schedule.TaskId
                    });
                }

                switch (jobMessage.JobStatus)
                {
                    case JobStatus.Running:
                        UpdateRunningJob(jobMessage);
                        break;

                    case JobStatus.DataIsNotReady:
                        UpdateDataNotReadyJob(jobMessage);
                        break;

                    case JobStatus.Success:
                        await SendMailForSuccessfulJob(jobMessage);
                        break;

                    case JobStatus.SuccessWithoutSendmail:
                        UpdateSuccessJobWithoutSendMail(jobMessage);
                        break;

                    case JobStatus.JobFailed:
                        UpdateFailedJob(jobMessage);
                        break;

                    case JobStatus.Abort:
                        UpdateAbortJob(jobMessage);
                        break;

                    default:
                        // Do nothing
                        break;
                }
            }
            catch (Exception ex)
            {
                logService.LogError(ex, jobMessage.Schedule);
            }
        }

        public void UpdateFailedForLongRunningMailTask()
        {
            try
            {
                mailRepository.UpdateFailedForLongRunningMailTask();
            }
            catch (Exception ex)
            {
                logService.LogError(ex);
            }
        }

        public int AddMailTaskForRunningJob(JobMessage jobMessage)
        {
            int taskId = 0;

            try
            {
                taskId = mailRepository.AddMailTask(new AddMailTaskCriteria
                {
                    MailId = jobMessage.Schedule.Id,
                    SiteId = jobMessage.Schedule.SiteId,
                    ScheduledTime = jobMessage.ScheduledTime,
                    Status = (int)JobStatus.Running,
                    Attempt = jobMessage.Schedule.CurrentRetryTimes,
                    LastSent = jobMessage.RunningTime
                });
            }
            catch (Exception ex)
            {
                logService.LogError(ex, jobMessage.Schedule);
            }

            return taskId;
        }

        public void InsertTaskLog(TaskLog log)
        {
            try
            {
                if (log.TaskId > 0)
                {
                    mailRepository.AddMailLog(new AddMailLogCriteria
                    {
                        LogTime = DateTime.Now,
                        ExceptionMessage = log.Message,
                        Status = log.Status,
                        TaskId = log.TaskId
                    });
                }
            }
            catch (Exception ex)
            {
                logService.LogError(ex);
            }
        }

        public MailTaskModel GetMailTaskById(GetMailTaskByIdRequest request)
        {
            MailTaskModel result = null;

            try
            {
                result = mailRepository.GetMailTaskById(new GetMailTaskByIdCriteria
                {
                    MailTaskId = request.TaskId
                });
            }
            catch (Exception ex)
            {
                logService.LogError(ex);

                result = new MailTaskModel();
            }

            return result;
        }

        public async Task SendMail(Mail mail, int mailTaskId = 0)
        {
            var mailRecordId = 0;
            Schedule schedule = null;
            int taskId = mailTaskId;
            var logger = new StringBuilder($"Server Logs:{Environment.NewLine}");
            string message = string.Empty;

            try
            {
                schedule = mailRepository.GetScheduleByMailId(new GetScheduleByMailIdCriteria { MailId = mail.MailingListId });

                if (schedule == null)
                {
                    throw new InvalidOperationException($"Mail #{mail.MailingListId} is not found.");
                }
                else if (schedule.Active)
                {
                    var mailRecord = ConvertFromMailToMailRecord(mail);

                    ValidateMail(mailRecord);

                    message = $"Inserting a mail record for the mail #{mail.MailingListId}";
                    logger.AppendLine($"> {message}");
                    logService.LogInfo(message);

                    mailRecordId = (int)mailRepository.InsertMailRecord(mailRecord);

                    if (schedule.TriggerTypeEnum == MailType.Function)
                    {
                        message = $"Inserting the task for the function mail #{mail.MailingListId}";
                        logger.AppendLine($"> {message}");
                        logService.LogInfo(message);

                        taskId = InsertMailTaskForFunctionMail(mail, mailRecordId);
                    }

                    if (mailRecordId > 0)
                    {
                        var retryMailRecord = new RetryMailRecord(mailRecord, sendMailRetryLimit);
                        mailRecord.IsReady = true;
                        mailRecord.MailID = mailRecordId;

                        bool isSuccessful = await SendMailRecord(retryMailRecord, mail.MailingListId, taskId, logger);

                        var status = isSuccessful ? JobStatus.Success : JobStatus.JobFailed;
                        var taskLog = new TaskLog
                        {
                            LogTime = DateTime.Now,
                            Message = logger.ToString()
                        };

                        InsertOrUpdateMailTaskAndLog(taskId, mail, mailRecordId, status, taskLog);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Failed to store a mail record for the mail #{mail.MailingListId}.");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Mail #{mail.MailingListId} is disabled.");
                }
            }
            catch (Exception ex)
            {
                logService.LogError(ex, schedule, mail.MailingListId);
                logger.AppendLine($"There is an error -> {ex.InnerException?.Message ?? ex.Message}");

                InsertOrUpdateMailTaskAndLog(
                    taskId,
                    mail,
                    mailRecordId,
                    JobStatus.JobFailed,
                    new TaskLog
                    {
                        LogTime = DateTime.Now,
                        Message = logger.ToString(),
                        StackTrace = ex.StackTrace
                    });
            }
        }

        private int InsertMailTaskForFunctionMail(Mail mail, int mailRecordId)
        {
            int taskId = mailRepository.AddMailTask(new AddMailTaskCriteria
            {
                MailId = mail.MailingListId,
                SiteId = mail.SiteId,
                ScheduledTime = DateTime.Now,
                Status = (int)JobStatus.Running,
                MSiteMailId = mailRecordId,
                LastSent = DateTime.Now
            });

            return taskId;
        }

        private void InsertOrUpdateMailTaskAndLog(
            int mailTaskId,
            Mail mail,
            int mailRecordId,
            JobStatus status,
            TaskLog taskLog = null)
        {
            try
            {
                int taskId = mailTaskId;

                if (taskId == 0)
                {
                    taskId = InsertMailTaskForFunctionMail(mail, mailRecordId);
                }

                if (taskId > 0)
                {
                    var updateTaskCriteria = new UpdateMailTaskStatusCriteria
                    {
                        TaskId = taskId,
                        Status = (int)status,
                        LastSent = DateTime.Now
                    };

                    mailRepository.UpdateMailTaskStatus(updateTaskCriteria);

                    if (mailRecordId > 0)
                    {
                        mailRepository.UpdateMailTask(new UpdateMailTaskMSiteMailIdCriteria
                        {
                            TaskId = taskId,
                            MSiteMailId = mailRecordId
                        });
                    }

                    if (taskLog != null)
                    {
                        mailRepository.AddMailLog(new AddMailLogCriteria
                        {
                            LogTime = taskLog.LogTime,
                            ExceptionMessage = taskLog.Message,
                            StackTrace = taskLog.StackTrace,
                            Status = (int)status,
                            TaskId = taskId
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logService.LogError(ex, mail.MailingListId);
            }
        }

        private async Task<bool> SendMailRecord(RetryMailRecord retryMailRecord, int mailId, int taskId, StringBuilder logger)
        {
            bool isSuccessful = true;
            string error = string.Empty;
            string message = string.Empty;
            var mailRecord = retryMailRecord.MailRecord;

            if (logger.Length == 0)
            {
                logger.AppendLine("Server Logs:");
            }

            var mailTask = GetMailTaskById(new GetMailTaskByIdRequest { TaskId = taskId });

            if (mailTask.Status != (int)JobStatus.Abort)
            {
                message = $"Sending out the mail record #{mailRecord.MailID} (mail #{mailId})";
                logger.AppendLine($"> {message}");
                logService.LogInfo(message);

                var record = mailRecord.IsReady ? mailRecord : mailRepository.GetMailRecord(mailRecord.MailID, true);

                if (record == null)
                {
                    throw new InvalidOperationException($"Cannot get mail record #{mailRecord.MailID} to send.");
                }

                using (var mailService = new Fanex.MailDeliver.MailClient.API.MailDeliver())
                {
                    mailService.SendMail(record);
                }

                if (!string.IsNullOrWhiteSpace(error))
                {
                    isSuccessful = false;
                    message = $"Failed to send the mail record #{mailRecord.MailID} (mail #{mailId}) -> Error: {error}";
                    logger.AppendLine($"> {message}");
                    logService.LogError(message);

                    retryMailRecord.RetryTime++;

                    if (retryMailRecord.CanRetry)
                    {
                        mailRecord.IsReady = false;
                        message = $"Resend({retryMailRecord.RetryTime}) the mail record #{mailRecord.MailID} (mail #{mailId}) in {sendMailRetryIntervalInMinutes} minutes...";
                        logger.AppendLine($"> {message}");
                        logService.LogInfo(message);

                        InsertTaskLog(mailId, taskId, logger);

                        await Task.Delay(TimeSpan.FromMinutes(sendMailRetryIntervalInMinutes));

                        isSuccessful = await SendMailRecord(retryMailRecord, mailId, taskId, logger);
                    }
                    else
                    {
                        message = $"Sending the mail record #{mailRecord.MailID} (mail #{mailId}) exceeded maximum retrying time({sendMailRetryLimit})";

                        logger.AppendLine($"> {message}");
                        logService.LogError(message);
                    }
                }
                else
                {
                    isSuccessful = true;

                    message = $"Succeeded to send the mail record #{mailRecord.MailID} (mail #{mailId})";
                    logger.AppendLine($"> {message}");
                    logService.LogInfo(message);
                }
            }
            else
            {
                isSuccessful = false;

                message = $"Not allow to send out the mail #{mailId} because it is aborted by user";
                logger.AppendLine($"> {message}");
                logService.LogInfo(message);
            }

            return isSuccessful;
        }

        private void InsertTaskLog(int mailId, int taskId, StringBuilder logger)
        {
            if (taskId > 0)
            {
                try
                {
                    mailRepository.AddMailLog(new AddMailLogCriteria
                    {
                        LogTime = DateTime.Now,
                        ExceptionMessage = logger.ToString(),
                        Status = (int)JobStatus.JobFailed,
                        TaskId = taskId
                    });

                    logger.Clear();
                }
                catch (Exception ex)
                {
                    logService.LogError(ex, mailId);
                }
            }
        }

        private static void ValidateMail(MailRecord mail)
        {
            if (string.IsNullOrWhiteSpace(mail.Subject))
            {
                throw new ArgumentNullException("mail.Subject", "Mail subject cannot be null or empty.");
            }
        }

        private void UpdateRunningJob(JobMessage jobMessage)
        {
            mailRepository.UpdateMailTaskStatus(new UpdateMailTaskStatusCriteria
            {
                TaskId = jobMessage.Schedule.TaskId,
                Status = (int)JobStatus.Running,
                LastSent = DateTime.Now
            });

            mailRepository.UpdateRetryCounter(new UpdateTaskRetryCounterCriteria
            {
                TaskId = jobMessage.Schedule.TaskId,
                RetryCount = jobMessage.Schedule.CurrentRetryTimes
            });
        }

        private void UpdateAbortJob(JobMessage jobMessage)
        {
            mailRepository.UpdateMailTaskStatus(new UpdateMailTaskStatusCriteria
            {
                TaskId = jobMessage.Schedule.TaskId,
                Status = (int)JobStatus.Abort,
                LastSent = DateTime.Now
            });
        }

        private void UpdateDataNotReadyJob(JobMessage jobMessage)
        {
            mailRepository.UpdateMailTaskStatus(new UpdateMailTaskStatusCriteria
            {
                TaskId = jobMessage.Schedule.TaskId,
                Status = (int)jobMessage.JobStatus,
                LastSent = DateTime.Now
            });
        }

        private async Task SendMailForSuccessfulJob(JobMessage jobMessage)
        {
            var mailTask = mailRepository.GetMailTaskById(new GetMailTaskByIdCriteria { MailTaskId = jobMessage.Schedule.TaskId });

            if (mailTask.Status != (int)JobStatus.Abort)
            {
                await SendMail(jobMessage.Mail, jobMessage.Schedule.TaskId);
            }
            else
            {
                string message = $"Not allow to send out the mail #{jobMessage.Schedule.Id} ({jobMessage.Schedule.Name}) because it is aborted by user";

                logService.LogInfo(message);
            }
        }

        private void UpdateSuccessJobWithoutSendMail(JobMessage jobMessage)
        {
            mailRepository.UpdateMailTaskStatus(new UpdateMailTaskStatusCriteria
            {
                TaskId = jobMessage.Schedule.TaskId,
                Status = (int)jobMessage.JobStatus,
                LastSent = DateTime.Now
            });

            // Because the JobRequest cannot be update to success when aborted. We will check Status again to make sure JobRequest is done.
            var mailTask = mailRepository.GetMailTaskById(new GetMailTaskByIdCriteria { MailTaskId = jobMessage.Schedule.TaskId });

            if (mailTask.Status == (int)jobMessage.JobStatus)
            {
                mailRepository.UpdateMailTask(new UpdateMailTaskMSiteMailIdCriteria
                {
                    TaskId = jobMessage.Schedule.TaskId,
                    MSiteMailId = 0
                });
            }
        }

        private void UpdateFailedJob(JobMessage jobMessage)
        {
            mailRepository.UpdateMailTaskStatus(new UpdateMailTaskStatusCriteria
            {
                TaskId = jobMessage.Schedule.TaskId,
                Status = (int)JobStatus.JobFailed,
                LastSent = DateTime.Now
            });
        }

        private MailRecord ConvertFromMailToMailRecord(Mail mail)
        {
            var mailingList = mailRepository.GetMailingList(new GetMailWithEmailAddressByMailId { MailId = mail.MailingListId });

            var mailRecord = new MailRecord
            {
                AppID = mail.ApplicationSendMailId,
                Config = new MailConfig
                {
                    AppID = mail.ApplicationSendMailId
                },
                Body = mail.Body,
                Subject = mail.Subject,
                Title = mail.Title,
                MailTo = string.Join(";", mailingList.TOs),
                MailCc = string.Join(";", mailingList.CCs),
                MailBcc = string.Join(";", mailingList.BCCs)
            };

            TransferAttachmentsToMailRecord(mail, mailRecord);

            TransferAlternateViewToMailRecord(mail, mailRecord);

            return mailRecord;
        }

        private static void TransferAlternateViewToMailRecord(Mail sourceMail, MailRecord destinationMailRecord)
        {
            if (sourceMail.AlternativeViews != null && sourceMail.AlternativeViews.Count > 0)
            {
                int alternativeViewsCount = sourceMail.AlternativeViews.Count;

                for (int i = 0; i < alternativeViewsCount; i++)
                {
                    var alternativeView = sourceMail.AlternativeViews[i];

                    destinationMailRecord.AlternateView.Add(new MailDeliver.MailClient.Model.MailLinkedResource
                    {
                        ContentCID = alternativeView.ContentId,
                        FileName = alternativeView.FileName,
                        ImageType = (FileExtentions)((int)alternativeView.FileExtension),
                        LinkedID = alternativeView.Id,
                        MailID = alternativeView.MailId,
                        Bytes = alternativeView.FileContent
                    });
                }
            }
        }

        private static void TransferAttachmentsToMailRecord(Mail sourceMail, MailRecord destinationMailRecord)
        {
            if (sourceMail.Attachments != null && sourceMail.Attachments.Count > 0)
            {
                int mailAttachCount = sourceMail.Attachments.Count;

                for (int i = 0; i < mailAttachCount; i++)
                {
                    var attach = sourceMail.Attachments[i];

                    destinationMailRecord.Attachments.Add(new MailDeliver.MailClient.Model.MailAttachment
                    {
                        AttachmentID = attach.Id,
                        Bytes = attach.FileContent,
                        ContentType = (FileExtentions)((int)attach.FileExtension),
                        FileName = attach.FileName,
                        MailID = attach.MailId
                    });
                }
            }
        }
    }
}