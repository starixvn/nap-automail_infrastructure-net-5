namespace Fanex.MailDelivery.Client.Configurations
{
    using System.Configuration;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ScheduleElement : ConfigurationElement
    {
        [ConfigurationProperty("id", IsKey = true, IsRequired = true)]
        public int Id
        {
            get { return (int)this["id"]; }
            set { this["id"] = value; }
        }

        [ConfigurationProperty("job", IsRequired = true)]
        public string Job
        {
            get { return (string)this["job"]; }
            set { this["job"] = value; }
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("JobInputData", IsRequired = false)]
        public JobInputDataElementCollection JobInputData
        {
            get { return (JobInputDataElementCollection)this["JobInputData"]; }
            set { this["JobInputData"] = value; }
        }
    }
}