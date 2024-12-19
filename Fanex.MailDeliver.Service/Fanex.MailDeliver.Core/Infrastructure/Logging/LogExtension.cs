// --------------------------------------------------------------------------
// <copyright file="LogExtension.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.Core.Infrastructure.Logging;

using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using Services;
using System;

public enum LoggingMode
{
    Console,
    Service
}

public static class LogExtension
{
    static LogExtension()
    {
        LoggingMode = LoggingMode.Service;
    }

    public static LoggingMode LoggingMode { get; set; }

    public static void InitLogging(LoggingMode mode)
    {
        LoggingMode = mode;
    }

    public static void LogError(Exception ex)
    {
        if (LoggingMode.Equals(LoggingMode.Console))
        {
            Console.WriteLine($"Error: {ex}");
        }
        else
        {
            var logMessage = new LogMessage(ex.Message)
                .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                .ToString();

            LogService.Error(logMessage);
        }
    }

    public static void LogMessage(string message)
    {
        if (LoggingMode.Equals(LoggingMode.Console))
        {
            Console.WriteLine($"Message: {message}");
        }
        else
        {
            if (AppConfigService.Config.IsAllowWritingLog)
            {
                LogService.Info(message);
            }
        }
    }
}