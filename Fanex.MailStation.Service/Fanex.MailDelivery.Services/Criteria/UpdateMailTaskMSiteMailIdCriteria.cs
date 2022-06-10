using Fanex.Data.Repository;

namespace Fanex.MailDelivery.Services.Criteria
{
    public class UpdateMailTaskMSiteMailIdCriteria : CriteriaBase
    {
        public int TaskId { get; set; }

        public int MSiteMailId { get; set; }

        public override bool IsValid()
        {
            return true;
        }

        public override string GetSettingKey()
        {
            return "Mail_Core_UpdateMailTaskMSiteMailID";
        }
    }
}
