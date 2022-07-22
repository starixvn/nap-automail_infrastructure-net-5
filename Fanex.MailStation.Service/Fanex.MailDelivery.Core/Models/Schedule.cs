using System;
using System.Runtime.InteropServices;

namespace Fanex.MailDelivery.Core.Models
{
    [Serializable]
    public class Schedule
    {
        // MailId
        public int Id { get; set; }

        public int SiteId { get; set; }

        public int TaskId { get; set; }

        public string Name { get; set; }

        public int TriggerType { get; set; }

        public string RepeatExpression { get; set; }

        /// <summary>
        /// It's not current fire time of system but the time admin want it to be.
        /// It's mean the task is run for schedule on the pass (Missing jobs).
        /// </summary>
        public DateTime? CurrentFireTime { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool Active { get; set; }

        public int RetryInterval { get; set; }

        public int MaxRetryTimes { get; set; }

        public int CurrentRetryTimes { get; set; }

        public bool AutoRetry { get; set; }

        public MailType TriggerTypeEnum
        {
            get
            {
                return TriggerType == 0 ? MailType.Function : MailType.Schedule;
            }

            set
            {
                TriggerType = (int)value;
            }
        }

        public Schedule Clone()
        {
            return new Schedule
            {
                Id = Id,
                SiteId = SiteId,
                TaskId = TaskId,
                Name = Name,
                RepeatExpression = RepeatExpression,
                StartTime = StartTime,
                EndTime = EndTime,
                Active = Active,
                RetryInterval = RetryInterval,
                MaxRetryTimes = MaxRetryTimes,
                CurrentRetryTimes = CurrentRetryTimes,
                CurrentFireTime = CurrentFireTime,
                TriggerType = TriggerType,
                TriggerTypeEnum = TriggerTypeEnum,
                AutoRetry = AutoRetry
            };
        }

        public bool CanRetry()
        {
            return CurrentRetryTimes <= MaxRetryTimes;
        }

        public bool IsScheduleEnd()
        {
            return EndTime != null && EndTime < DateTime.Now;
        }

        public void ConvertToServerTime(bool isDevEnvironment)
        {
            int hoursDifferent = CalculateHoursDifferentBeetweenDevAndServer(isDevEnvironment);

            StartTime = StartTime.AddHours(hoursDifferent);

            if (EndTime != null)
            {
                EndTime = EndTime.Value.AddHours(hoursDifferent);
            }

            ConvertCronExpressionToServerTime(hoursDifferent);
        }

        private void ConvertCronExpressionToServerTime(int hoursDifferent)
        {
            string[] cron = RepeatExpression.Split(' ');
            int hour = int.Parse(cron[2]);

            hour += hoursDifferent;

            while (hour > 23)
            {
                hour -= 24;
            }

            while (hour < 0)
            {
                hour += 24;
            }

            cron[2] = hour.ToString();

            RepeatExpression = string.Join(" ", cron);
        }

        private static int CalculateHoursDifferentBeetweenDevAndServer(bool isDevEnvironment)
        {
            string timezoneName = isDevEnvironment ? MailDeliveryConstants.DevServerTimeZone : MailDeliveryConstants.LiveServerTimeZone;
            DateTime nowDate = DateTime.Now;

#if !NETFRAMEWORK
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                timezoneName = TimeZoneConverter.TZConvert.WindowsToIana(timezoneName);
            }
#endif
            return -(TimeZoneInfo.ConvertTime(nowDate, TimeZoneInfo.FindSystemTimeZoneById(timezoneName)) - nowDate).Hours;
        }
    }
}