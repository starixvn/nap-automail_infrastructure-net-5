namespace Fanex.MailDelivery.Core.Models;

public class ConfigSettingModel
{
    public string AppMailId { get; set; }

    public string AppMailAddress { get; set; }

    public string RabbitMQConnectionString { get; set; }

    public int SendMailRetryLimit { get; set; }

    public int SendMailRetryIntervalInMinutes { get; set; }

    public int UpdateFailedForLongRunningTaskIntervalInMinutes { get; set; }

    public int UMInfoCachingTimeoutInSeconds { get; set; }

    public bool IsAllowWritingLog { get; set; }
}