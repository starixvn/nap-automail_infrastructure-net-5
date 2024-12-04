using System;

namespace Fanex.MailDelivery.Core.Models;

/// <summary>
/// JobRequest Status Message.
/// </summary>
public class JobMessage
{
    public Schedule Schedule { get; set; }

    public Mail Mail { get; set; }

    public JobStatus JobStatus { get; set; }

    public TaskLog TaskLog { get; set; }

    public DateTime ScheduledTime { get; set; }

    public DateTime RunningTime { get; set; }
}