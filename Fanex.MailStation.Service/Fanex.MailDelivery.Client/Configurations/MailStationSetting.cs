using Fanex.MailDelivery.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fanex.MailDelivery.Client.Configurations;

public class MailStationSetting
{
    private string rabbitMQConnectionString;

    public int ApplicationSendMailId { get; set; }

    public int SiteId { get; set; }

    public string RabbitMQConnectionString
    {
        get
        {
            const string pattern = "\btimeout=[0-9]+\b;*";
            var connection = $"{rabbitMQConnectionString}{MailDeliveryConstants.RabbitMQLoginPassword}";

            if (!Regex.IsMatch(connection, pattern, RegexOptions.IgnoreCase))
            {
                connection = $"{connection}{MailDeliveryConstants.RabbitMQTimeout}";
            }

            return connection;
        }
        set => rabbitMQConnectionString = value;
    }

    public string Environment { get; set; }

    public int RequestRetryLimit { get; set; } = 20;

    public int RequestRetryIntervalInSeconds { get; set; } = 120;

    public int UpdateSchedulesIntervalInSeconds { get; set; } = 300;

    public List<FunctionSetting> Functions { get; set; } = [];

    public List<ScheduleSetting> Schedules { get; set; } = [];

    public bool HasSchedule()
    {
        return Schedules.Count > 0;
    }

    public bool IsDevEnvironment()
    {
        var environment = Environment ?? string.Empty;
        var isDev = environment.Trim().Equals("dev", StringComparison.OrdinalIgnoreCase);

        return isDev;
    }

    public int GetFunctionId(string functionName)
    {
        var function = Functions.FirstOrDefault(x => x.Name.Equals(functionName, StringComparison.OrdinalIgnoreCase));

        return function?.Id ?? 0;
    }

    public Type GetJobType(int scheduleId)
    {
        var schedule = Schedules.FirstOrDefault(x => x.Id == scheduleId);
        var jobType = schedule != null ? Type.GetType(schedule.Job.Trim()) : null;

        return jobType;
    }

    public bool HasScheduleId(int scheduleId) => Schedules.Any(x => x.Id == scheduleId);
}

public class FunctionSetting
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public class ScheduleSetting : FunctionSetting
{
    public string Job { get; set; }

    public List<JobInputDataSetting> JobInputData { get; set; } = [];
}

public class JobInputDataSetting
{
    public string Value { get; set; }

    public string Name { get; set; }
}