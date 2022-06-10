using System;

namespace Fanex.MailDelivery.Core.Models
{
    public class MailTaskModel
    {
        public int MailTaskId { get; set; }

        public int MailId { get; set; }

        public string MailName { get; set; }

        public int TriggerType { get; set; }

        public int SiteId { get; set; }

        public string SiteName { get; set; }

        public DateTime ScheduledTime { get; set; }

        public DateTime? LastSent { get; set; }

        public int Status { get; set; }

        public int Attempt { get; set; }

        public int MSiteMailId { get; set; }

        public bool IsUnableToRetry { get; set; }
    }
}
