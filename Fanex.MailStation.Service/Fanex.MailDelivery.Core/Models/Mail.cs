using System.Collections.Generic;

namespace Fanex.MailDelivery.Core.Models
{
    /// <summary>
    /// Mail content.
    /// </summary>
    public class Mail
    {
        public int SiteId { get; set; }

        public int MailingListId { get; set; }

        public int ApplicationSendMailId { get; set; }

        public int LicenseeId { get; set; }

        #region Mail Content

        public long Id { get; set; }

        public string Subject { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public List<MailAttachment> Attachments { get; set; }

        public List<MailLinkedResource> AlternativeViews { get; set; }

        public bool IsEncrypt { get; set; }

        #endregion
    }
}
