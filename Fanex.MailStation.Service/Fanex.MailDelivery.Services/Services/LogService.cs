using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.MailDelivery.Core;
using Fanex.MailDelivery.Core.Models;
using System;
using System.Collections.Generic;

namespace Fanex.MailDelivery.Services.Services
{
    public interface LogService
    {
        void LogInfo(string logMessage);

        void LogError(string message);

        void LogError(string message, Exception exception);

        void LogError(Exception exception);

        void LogError(Exception ex, int mailId);

        void LogError(Exception ex, List<int> scheduleIds);

        void LogError(Exception ex, Schedule schedule);

        void LogError(Exception ex, Schedule schedule, int mailId);
    }

    public class LogServiceImpl : LogService
    {
        private readonly ILogger logger;

        public LogServiceImpl() : this(null)
        {
        }

        public LogServiceImpl(ILogger logger)
        {
            this.logger = logger ?? Logger.Log;
        }

        public void LogInfo(string logMessage)
        {
            if (AppConfigService.AppConfig.IsAllowWritingLog)
            {
                logger.Info(logMessage);
            }
        }

        public void LogError(string message)
        {
            var logMessage = new LogMessage(message)
                        .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                        .ToString();

            logger.Error(logMessage);
        }

        public void LogError(string message, Exception exception)
        {
            var logMessage = new LogMessage(message)
                        .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                        .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
                        .ToString();

            logger.Error(logMessage);
        }

        public void LogError(Exception exception)
        {
            LogError(exception.Message, exception);
        }

        public void LogError(Exception ex, Schedule schedule)
        {
            if (schedule != null)
            {
                string message = $@"{(schedule.TriggerTypeEnum == MailType.Function ? "Function" : "Schedule")} Mail #{schedule.Id} ({schedule.Name})
                                    {Environment.NewLine}Error: {ex.Message}";

                LogError(message, ex);
            }
            else
            {
                LogError(ex);
            }
        }

        public void LogError(Exception ex, int mailId)
        {
            string message = $@"Schedule Mail #{mailId}{Environment.NewLine}Error: {ex.Message}";

            LogError(message, ex);
        }

        public void LogError(Exception ex, List<int> scheduleIds)
        {
            string message = $@"Mail #{string.Join(", ", scheduleIds)}{Environment.NewLine}Error: {ex.Message}";

            LogError(message, ex);
        }

        public void LogError(Exception ex, Schedule schedule, int mailId)
        {
            if (schedule != null)
            {
                LogError(ex, schedule);
            }
            else
            {
                LogError(ex, mailId);
            }
        }
    }
}