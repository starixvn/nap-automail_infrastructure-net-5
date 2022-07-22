using Fanex.Data.Repository;

namespace Fanex.MailDelivery.Services.Criteria.Job
{
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
}
