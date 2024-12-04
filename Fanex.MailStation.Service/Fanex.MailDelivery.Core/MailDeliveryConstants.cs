namespace Fanex.MailDelivery.Core;

public static class MailDeliveryConstants
{
    public const string SendMailRoutingKey = "RoutingKey.SendMail";
    public const string JobResultRoutingKey = "RoutingKey.JobResult";
    public const string TaskLogRoutingKey = "RoutingKey.TaskLog";
    public const string UpdateScheduleRoutingKeyFormat = "RoutingKey.UpdateSchedule_{0}";
    public const string RetryJobRoutingKeyFormat = "RoutingKey.RetryJob_{0}";
    public const string AbortJobRoutingKeyFormat = "RoutingKey.AbortJob";
    public const string RunNowJobRoutingKeyFormat = "RoutingKey.RunNowJob_{0}";
    public const string RunAfterUMJobRoutingKeyFormat = "RoutingKey.RunAfterUMJob_{0}";

    public const string AutoMailJobNameFormat = "AutoMailJob_{0}";
    public const string MailCentralizedTriggerGroup = "MailStationClient.TriggerGroup";

    public const string ScheduleJobDataKey = "MailStationClient.ScheduleJobDataKey";
    public const string RabbitMQLoginPassword = ";password=1q2w3e$R";
    public const string RabbitMQTimeout = ";timeout=120";

    public const string ClientNotificationJobName = "ClientNotificationJobName";
    public const string ClientNotificationTriggerName = "ClientNotificationTriggerName";
    public const string ClientNotificationTopic = "ClientNotificationTopic";

    public const int ClientNotificationIntervalTimeInSecond = 10;

    public const string DevServerTimeZone = "SE Asia Standard Time"; // (UTC+07:00) Bangkok, Hanoi, Jakarta   
    public const string LiveServerTimeZone = "SA Western Standard Time"; //(UTC-04:00) Georgetown, La Paz, Manaus, San Juan
}