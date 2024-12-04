namespace Fanex.MailDelivery.Client.Configurations;

using System.Configuration;

/// <summary>
/// JobRequest Input Data Element Collection.
/// </summary>
[ConfigurationCollection(typeof(JobInputDataElement))]
public class JobInputDataElementCollection : ConfigurationElementCollection
{
    protected override ConfigurationElement CreateNewElement()
    {
        return new JobInputDataElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
        return ((JobInputDataElement)element).Name;
    }
}