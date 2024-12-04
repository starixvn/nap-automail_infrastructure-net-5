using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.MailDelivery.Core;
using Fanex.MailDelivery.Core.Models;
using System;

namespace Fanex.MailDelivery.Client.Logging;

internal static class LogService
{
    public static void LogInfo(string logMessage)
    {
        AppConfigService.AppConfig ??= new ConfigSettingModel { IsAllowWritingLog = true };
        if (AppConfigService.AppConfig.IsAllowWritingLog)
        {
            Logger.Log.Info(logMessage);
        }
    }

    public static void LogError(Exception exception)
    {
        Logger.Log.Error(null, exception);
    }

    public static void LogError(string message, Exception exception)
    {
        string errorMessage = $"{(string.IsNullOrWhiteSpace(message) ? "Error:" : $"{message} -> ")} {exception.InnerException?.Message ?? exception.Message}";

        var logMessage = new LogMessage(errorMessage)
            .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
            .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
            .ToString();

        Logger.Log.Error(logMessage);
    }

    public static void LogError(Schedule schedule, Exception ex)
    {
        if (schedule != null)
        {
            string message = $"Schedule Mail #{schedule.Id} ({schedule.Name})";

            LogError(message, ex);
        }
        else
        {
            LogError(ex);
        }
    }

    public static void LogError(string message, Schedule schedule, Exception ex)
    {
        string errorMessage = schedule != null
            ? $"Schedule Mail #{schedule.Id} ({schedule.Name}){Environment.NewLine}Error: {message}"
            : message;

        LogError(errorMessage, ex);
    }

    public static void LogError(string message, string mailIds, Exception ex)
    {
        string errorMessage = !string.IsNullOrWhiteSpace(mailIds)
            ? $"Mails #{mailIds}{Environment.NewLine}Error: {message}"
            : message;

        LogError(errorMessage, ex);
    }

    public static void LogError(string message)
    {
        var logMessage = new LogMessage(message)
            .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
            .ToString();

        Logger.Log.Error(logMessage);
    }
}