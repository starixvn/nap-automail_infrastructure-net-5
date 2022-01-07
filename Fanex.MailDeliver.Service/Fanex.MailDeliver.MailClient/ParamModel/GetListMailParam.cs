using System;

namespace Fanex.MailDeliver.MailClient.ParamModel
{
    public class GetListMailParam
    {
        public int AppId { get; set; }

        public DateTime MailDate { get; set; }

        public int Page { get; set; }

        public int Counter { get; set; }
    }
}