using EasyNetQ;
using Fanex.MailDeliver.Core.Infrastructure.Models;
using Fanex.MailDeliver.MailClient.Model;
using System;
using System.Collections.Generic;

namespace Fanex.MailDeliver.Core.Infrastructure.Services;

public static class AppConfigService
{
    public static ConfigSettingModel Config { get; set; }

    public static IBus Bus { get; set; }

    public static object ListMailLockObject { get; set; } = new();

    public static List<MailRecord> ListMailFailed { get; set; } = [];

    public static List<DateTime> TimeCounter = [];

    public static DateTime LastTimeSendMail { get; set; } = DateTime.Now;
}