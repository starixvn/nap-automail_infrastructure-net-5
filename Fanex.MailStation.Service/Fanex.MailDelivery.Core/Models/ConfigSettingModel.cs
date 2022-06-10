using ONEbook.UM.Configurations;

namespace Fanex.MailDelivery.Core.Models
{
    public class ConfigSettingModel
    {
        public string SentryClientKey { get; set; }

        public string CategorySentryLog { get; set; }

        public UMClientConfiguration UMConfig { get; set; }

        public string AppMailId { get; set; }

        public string AppMailAddress { get; set; }

        public string RabbitMQConnectionString { get; set; }

        public int SendMailRetryLimit { get; set; }

        public int SendMailRetryIntervalInMinutes { get; set; }

        public int UpdateFailedForLongRunningTaskIntervalInMinutes { get; set; }

        public int UMInfoCachingTimeoutInSeconds { get; set; }
    }
}