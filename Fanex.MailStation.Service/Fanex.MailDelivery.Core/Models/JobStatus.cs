namespace Fanex.MailDelivery.Core.Models
{
    /// <summary>
    /// JobRequest Status.
    /// </summary>
    public enum JobStatus 
    {
        /// <summary>
        /// The waiting status.
        /// </summary>
        Waiting = 0,

        /// <summary>
        /// The running status.
        /// </summary>
        Running = 1,

        /// <summary>
        /// The data is not ready status.
        /// </summary>
        DataIsNotReady = 2,

        /// <summary>
        /// The failed status.
        /// </summary>
        JobFailed = 3,

        /// <summary>
        /// The success status.
        /// </summary>
        Success = 4,

        /// <summary>
        /// The abort status.
        /// </summary>
        Abort = 6,

        /// <summary>
        /// The success without send mail status.
        /// </summary>
        SuccessWithoutSendmail = 7,

        /// <summary>
        /// The missing job status.
        /// </summary>
        Missing = 8
    }
}
