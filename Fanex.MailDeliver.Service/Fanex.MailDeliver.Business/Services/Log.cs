using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using System;

namespace Fanex.MailDeliver.Business.Services
{
    public interface Log
    {
        void Log(Exception exception);

        void Log(string message);
    }

    public class LogImp : Log
    {
        public void Log(string message)
        {
            LogService.Info(message);
        }

        public void Log(Exception exception)
        {
            var logMessage = new LogMessage(exception.Message)
                        .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
                        .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                        .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
                        .ToString();

            LogService.Error(logMessage);
        }
    }
}