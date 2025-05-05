namespace Fanex.SimpleMailDelivery.Models;

public class SettingModel
{
    public int ApplicationSendMailId { get; set; }

    public int SiteId { get; set; }

    public string Hosting { get; set; }

    public string VirtualHost { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }
}