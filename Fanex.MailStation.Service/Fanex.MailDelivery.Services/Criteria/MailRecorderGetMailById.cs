using Fanex.Data.Repository;

namespace Fanex.MailDelivery.Services.Criteria;

public class MailRecorderGetMailById : CriteriaBase
{
    public int MailId { get; set; }

    public override string GetSettingKey()
    {
        return "GetMail";
    }

    public override bool IsValid() => true;
}