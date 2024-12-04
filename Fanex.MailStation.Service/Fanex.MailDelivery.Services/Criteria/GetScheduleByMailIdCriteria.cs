namespace Fanex.MailDelivery.Services.Criteria;

using Data.Repository;

public class GetScheduleByMailIdCriteria : CriteriaBase
{
    public int MailId { get; set; }

    public override bool IsValid()
    {
        return true;
    }

    public override string GetSettingKey()
    {
        return "Mail_MailManagement_GetScheduleByMailId";
    }
}