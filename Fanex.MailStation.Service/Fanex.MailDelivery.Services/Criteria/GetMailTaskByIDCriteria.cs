using Fanex.Data.Repository;

namespace Fanex.MailDelivery.Services.Criteria;

public class GetMailTaskByIdCriteria : CriteriaBase
{
    public int MailTaskId { get; set; }

    public override bool IsValid()
    {
        return true;
    }

    public override string GetSettingKey()
    {
        return "Mail_Core_GetMailTask";
    }
}