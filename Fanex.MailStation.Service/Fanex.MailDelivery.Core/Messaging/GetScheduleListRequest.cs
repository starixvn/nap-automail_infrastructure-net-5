using System.Collections.Generic;

namespace Fanex.MailDelivery.Core.Messaging;

public class GetScheduleListRequest
{
    public List<int> MailIds { get; set; } = [];
}