using Fanex.Data.Repository;

namespace Fanex.MailDelivery.Services.Criteria
{
    public class UpdateTaskRetryCounterCriteria : CriteriaBase
    {
        public int TaskId { get; set; }

        public int RetryCount { get; set; }

        public override bool IsValid()
        {
            return true;
        }

        public override string GetSettingKey()
        {
            return "Mail_Core_UpdateRetry";
        }
    }
}
