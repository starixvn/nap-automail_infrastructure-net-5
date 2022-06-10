namespace Fanex.MailDelivery.Client.Configurations
{
    using System.Configuration;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [ConfigurationCollection(typeof(ScheduleElement), AddItemName = "Schedule")]
    public class ScheduleElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ScheduleElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ScheduleElement)element).Id;
        }
    }
}