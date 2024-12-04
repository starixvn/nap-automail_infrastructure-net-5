namespace Fanex.MailDelivery.Client.Configurations;

using System.Configuration;

/// <summary>
/// TODO: Update summary.
/// </summary>
public class FunctionElement : ConfigurationElement
{
    [ConfigurationProperty("id", IsKey = true, IsRequired = true)]
    public int Id
    {
        get { return (int)this["id"]; }
        set { this["id"] = value; }
    }

    [ConfigurationProperty("name", IsRequired = true)]
    public string Name
    {
        get { return (string)this["name"]; }
        set { this["name"] = value; }
    }
}