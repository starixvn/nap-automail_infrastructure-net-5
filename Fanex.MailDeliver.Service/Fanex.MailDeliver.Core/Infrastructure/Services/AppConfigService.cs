using EasyNetQ;
using Fanex.MailDeliver.Core.Infrastructure.Models;
using Fanex.MailDeliver.Model;
using System;
using System.Collections.Generic;

namespace Fanex.MailDeliver.Core.Infrastructure.Services
{
    public static class AppConfigService
    {
        public static ConfigSettingModel Config { get; set; }

        public static IBus Bus { get; set; }

        public static object ListMailLockObject { get; set; } = new object();

        public static List<MailRecord> ListMailFailed { get; set; } = new List<MailRecord>();

        public static List<DateTime> TimeCounter = new List<DateTime>();
    }
}