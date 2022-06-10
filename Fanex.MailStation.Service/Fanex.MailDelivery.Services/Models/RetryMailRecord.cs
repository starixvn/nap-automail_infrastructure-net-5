using Fanex.MailDeliver.Model;

namespace Fanex.MailDelivery.Services.Models
{
    public class RetryMailRecord
    {
        public RetryMailRecord(MailRecord mailRecord, int maxRetryCount)
        {
            MailRecord = mailRecord;
            MaxRetryCount = maxRetryCount;
        }

        public MailRecord MailRecord { get; private set; }

        public int RetryTime { get; set; }

        public int MaxRetryCount { get; private set; }

        public bool CanRetry => RetryTime <= MaxRetryCount;
    }
}