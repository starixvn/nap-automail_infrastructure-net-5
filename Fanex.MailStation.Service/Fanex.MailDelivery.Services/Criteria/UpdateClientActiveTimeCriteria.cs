using Fanex.Data.Repository;

namespace Fanex.MailDelivery.Services.Criteria;

public class UpdateClientActiveTimeCriteria : CriteriaBase
{
    public string MailIds { get; set; }

    public override bool IsValid()
    {
        return true;
    }

    public override string GetSettingKey()
    {
        return "Mail_Core_ClientActiveTimeUpdate";
    }
}