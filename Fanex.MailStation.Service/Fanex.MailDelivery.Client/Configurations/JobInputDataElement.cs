namespace Fanex.MailDelivery.Client.Configurations;

using System.Configuration;

/// <summary>
/// JobRequest Input Data Element.
/// </summary>
public class JobInputDataElement : ConfigurationElement
{
    [ConfigurationProperty("value", IsRequired = true)]
    public string Value
    {
        get { return (string)this["value"]; }
        set { this["value"] = value; }
    }

    [ConfigurationProperty("name", IsRequired = true)]
    public string Name
    {
        get { return (string)this["name"]; }
        set { this["name"] = value; }
    }
}