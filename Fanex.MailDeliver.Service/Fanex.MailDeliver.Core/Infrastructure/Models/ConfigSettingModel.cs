namespace Fanex.MailDeliver.Core.Infrastructure.Models
{
    public class ConfigSettingModel
    {
        public string SentryClientKey { get; set; }

        public string CategorySentryLog { get; set; }

        public int NumberOfResendMail { get; set; }

        public int IntervalResendMail { get; set; }

        public string CronScheduleResendMail { get; set; }

        public string RabbitMQConnectionString { get; set; }

        public MailSection MailSection { get; set; }

        public bool IsAllowWritingLog { get; set; }

        public bool IsIgnoredRabbitMq { get; set; }
    }
}