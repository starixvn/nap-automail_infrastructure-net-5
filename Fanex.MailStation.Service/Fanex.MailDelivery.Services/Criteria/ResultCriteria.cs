namespace Fanex.MailDelivery.Services.Criteria
{
    using Fanex.Data.Repository;

    public class ResultCriteria : CriteriaBase
    {
        public ResultCriteria()
        {
            ErrCode = 0;
            ErrMsg = string.Empty;
        }

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        public override string GetSettingKey() => string.Empty;

        public override bool IsValid() => true;
    }
}