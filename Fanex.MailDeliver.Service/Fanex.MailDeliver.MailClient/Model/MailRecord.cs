// --------------------------------------------------------------------------
// <copyright file="MailRecord.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
//   <change who="Ricky.Nguyen" date="2012.9.24">Update MailStatus Enum for getting list MailRecord</change>
// </history>

using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Fanex.MailDeliver.MailClient.Model
{
    public enum MailStatus : int
    {
        /// <summary>
        /// All Mail Type.
        /// </summary>
        All = 0,

        /// <summary>
        /// Success Mail Type.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Fail Mail Type.
        /// </summary>
        Fail = -1
    }

    [Serializable]
    public class MailRecord
    {
        private bool _isReady = false;

        public MailRecord()
        {
            MailID = 0;
            Config = new MailConfig();
            Function = new MailFunction();
            MailTo = string.Empty;
            Title = string.Empty;
            Body = string.Empty;
            Subject = string.Empty;
            MailTo = string.Empty;
            MailCc = string.Empty;
            MailBcc = string.Empty;
            Attachments = new List<MailAttachment>();
            AlternateView = new List<MailLinkedResource>();
            SentFail = 0;
            SentSuccess = 0;
            CanRetry = true;
            MailFrom = string.Empty;
            IsEncrypt = false;
        }

        public long MailID
        {
            get;
            set;
        }

        public MailConfig Config
        {
            get;
            set;
        }

        public int AppID
        {
            get;
            set;
        }

        public int FuncID
        {
            get;
            set;
        }

        public MailFunction Function
        {
            get;
            set;
        }

        public string MailTo
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string Subject
        {
            get;
            set;
        }

        public string Body
        {
            get;
            set;
        }

        public string MailCc
        {
            get;
            set;
        }

        public string MailFrom
        {
            get;
            set;
        }

        public string MailBcc
        {
            get;
            set;
        }

        public string FileAttachments
        {
            get;
            set;
        }

        public List<MailAttachment> Attachments
        {
            get;
            set;
        }

        public string MailDate
        {
            get;
            set;
        }

        public string MailUpdatedDate
        {
            get;
            set;
        }

        public List<MailLinkedResource> AlternateView
        {
            get;
            set;
        }

        public bool IsReady
        {
            get
            {
                return _isReady;
            }

            set
            {
                _isReady = value;
            }
        }

        public int CountSend
        {
            get;
            set;
        }

        public int SentSuccess { get; set; }

        public int SentFail { get; set; }

        public bool CanRetry { get; set; }

        public int ResentCount { get; set; }

        public bool IsEncrypt
        {
            get;
            set;
        }

        public MailPriority Priority
        {
            get;
            set;
        }
    }
}