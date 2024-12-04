using Fanex.Data.Repository;
using System;

namespace Fanex.MailDelivery.Services.Criteria;

public class UpdateMailTaskStatusCriteria : CriteriaBase
{
    public int TaskId { get; set; }

    public int Status { get; set; }

    public DateTime? LastSent { get; set; } 

    public override bool IsValid()
    {
        return true;
    }

    public override string GetSettingKey()
    {
        return "Mail_Core_UpdateMailStatus";
    }
}