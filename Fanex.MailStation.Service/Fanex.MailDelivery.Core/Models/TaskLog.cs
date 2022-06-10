using System;

namespace Fanex.MailDelivery.Core.Models
{
    public class TaskLog
    {
        public int TaskId { get; set; }

        public string Message { get; set; }

        public DateTime LogTime { get; set; }

        public string StackTrace { get; set; }

        public int Status { get; set; }
    }
}
