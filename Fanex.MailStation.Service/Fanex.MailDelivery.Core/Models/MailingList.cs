using System.Collections.Generic;

namespace Fanex.MailDelivery.Core.Models;

/// <summary>
/// Mailing list.
/// </summary>
public class MailingList
{
    public int Id { get; set; }

    public int SiteId { get; set; }

    public string Name { get; set; }

    public int Group { get; set; }

    public bool Active { get; set; }

    public MailType MailType { get; set; }

    public List<string> TOs { get; set; }

    public List<string> CCs { get; set; }

    public List<string> BCCs { get; set; }
}