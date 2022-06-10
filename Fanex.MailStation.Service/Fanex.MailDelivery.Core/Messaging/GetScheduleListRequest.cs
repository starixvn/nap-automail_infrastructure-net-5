using System.Collections.Generic;

namespace Fanex.MailDelivery.Core.Messaging
{
    public class GetScheduleListRequest
    {
        public GetScheduleListRequest()
        {
            MailIds = new List<int>();
        }

        public List<int> MailIds { get; set; }
    }
}
