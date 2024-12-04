using Fanex.Data.Repository;
using System;

namespace Fanex.MailDelivery.Services.Criteria;

public class AddMailLogCriteria : CriteriaBase
{
    public int TaskId { get; set; }

    public string ExceptionMessage { get; set; }

    public string StackTrace { get; set; }

    public int Status { get; set; }

    public DateTime LogTime { get; set; }

    public override bool IsValid()
    {
        return true;
    }

    public override string GetSettingKey()
    {
        return "Mail_Core_InsertMailLogs";
    }
}