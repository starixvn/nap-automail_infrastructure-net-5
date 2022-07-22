using Fanex.MailDelivery.Core;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Fanex.MailDelivery.Client.Configurations
{
    public class MailClientSection : ConfigurationSection
    {
        [ConfigurationProperty("applicationSendMailId", IsRequired = true)]
        public int ApplicationSendMailId
        {
            get { return (int)this["applicationSendMailId"]; }
            set { this["applicationSendMailId"] = value; }
        }

        [ConfigurationProperty("environment", IsRequired = false)]
        public string Environment
        {
            get { return (string)this["environment"]; }
            set { this["environment"] = value; }
        }

        [ConfigurationProperty("Functions", IsRequired = false)]
        public FunctionElementCollection Functions
        {
            get { return (FunctionElementCollection)this["Functions"]; }
            set { this["Functions"] = value; }
        }

        [ConfigurationProperty("rabbitMQConnectionString", IsRequired = true)]
        public string RabbitMQConnectionString
        {
            get
            {
                const string pattern = "\btimeout=[0-9]+\b;*";
                var connection = $"{this["rabbitMQConnectionString"]}{MailDeliveryConstants.RabbitMQLoginPassword}";

                if (!Regex.IsMatch(connection, pattern, RegexOptions.IgnoreCase))
                {
                    connection = $"{connection}{MailDeliveryConstants.RabbitMQTimeout}";
                }

                return connection;
            }
            set { this["rabbitMQConnectionString"] = value; }
        }

        [ConfigurationProperty("requestRetryLimit", IsRequired = false, DefaultValue = 20)]
        public int RequestRetryLimit
        {
            get { return (int)this["requestRetryLimit"]; }
            set { this["requestRetryLimit"] = value; }
        }

        [ConfigurationProperty("requestRetryIntervalInSeconds", IsRequired = false, DefaultValue = 120)]
        public int RequestRetryIntervalInSeconds
        {
            get { return (int)this["requestRetryIntervalInSeconds"]; }
            set { this["requestRetryIntervalInSeconds"] = value; }
        }

        [ConfigurationProperty("updateSchedulesIntervalInSeconds", IsRequired = false, DefaultValue = 300)]
        public int UpdateSchedulesIntervalInSeconds
        {
            get { return (int)this["updateSchedulesIntervalInSeconds"]; }
            set { this["updateSchedulesIntervalInSeconds"] = value; }
        }

        [ConfigurationProperty("Schedules", IsRequired = false)]
        public ScheduleElementCollection Schedules
        {
            get { return (ScheduleElementCollection)this["Schedules"]; }
            set { this["Schedules"] = value; }
        }

        [ConfigurationProperty("siteId", IsRequired = true)]
        public int SiteId
        {
            get { return (int)this["siteId"]; }
            set { this["siteId"] = value; }
        }

        [ConfigurationProperty("enableQuartzLog", IsRequired = false, DefaultValue = false)]
        public bool EnableQuartzLog
        {
            get { return (bool)this["enableQuartzLog"]; }
            set { this["enableQuartzLog"] = value; }
        }

        public static MailClientSection GetConfig(string sectionName = "Fanex.MailDelivery.Client")
        {
            return ConfigurationManager.GetSection(sectionName) as MailClientSection;
        }

        public bool HasSchedule()
        {
            return Schedules.Count > 0;
        }

        public static bool IsDevEnvironment()
        {
            string environment = GetConfig().Environment ?? string.Empty;
            bool isDev = environment.Trim().Equals("dev", System.StringComparison.OrdinalIgnoreCase);

            return isDev;
        }
    }
}