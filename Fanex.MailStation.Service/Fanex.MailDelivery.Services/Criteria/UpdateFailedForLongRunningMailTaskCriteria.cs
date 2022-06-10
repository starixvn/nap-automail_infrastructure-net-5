using Fanex.Data.Repository;

namespace Fanex.MailDelivery.Services.Criteria
{
    public class UpdateFailedForLongRunningMailTaskCriteria : CriteriaBase
    {
        public override bool IsValid()
        {
            return true;
        }

        public override string GetSettingKey()
        {
            return "Mail_Core_UpdateFailedForLongRunningTasks";
        }
    }
}
