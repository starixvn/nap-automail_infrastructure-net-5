using Fanex.Data.Repository;

namespace Fanex.MailDelivery.Services.Criteria;

public class GetMailWithEmailAddressByMailId : CriteriaBase
{
    public int MailId { get; set; }

    public override bool IsValid()
    {
        return true;
    }

    public override string GetSettingKey()
    {
        return "Mail_MailManagement_GetMailWithEmailAddressByMailID";
    }
}