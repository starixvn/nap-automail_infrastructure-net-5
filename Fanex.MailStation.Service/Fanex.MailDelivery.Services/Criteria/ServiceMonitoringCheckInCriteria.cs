using Fanex.Data.Repository;

namespace Fanex.MailDelivery.Services.Criteria
{
    public class ServiceMonitoringCheckInCriteria : CriteriaBase
    {
        public string ServiceName { get; set; }

        public int Interval { get; set; }

        public int OrderBy { get; set; }

        public string IPServer { get; set; }

        public override bool IsValid() => true;

        public override string GetSettingKey() => "ServiceMonitoring_CheckIn";
    }
}