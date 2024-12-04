using Fanex.MailDeliver.MailClient.Model;

namespace Fanex.MailDelivery.Services.Models;

public class RetryMailRecord(MailRecord mailRecord, int maxRetryCount)
{
    public MailRecord MailRecord { get; private set; } = mailRecord;

    public int RetryTime { get; set; }

    public int MaxRetryCount { get; private set; } = maxRetryCount;

    public bool CanRetry => RetryTime <= MaxRetryCount;
}