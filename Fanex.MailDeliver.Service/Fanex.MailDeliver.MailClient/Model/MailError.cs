// --------------------------------------------------------------------------
// <copyright file="MailError.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2013.7.10">Created</change>
// </history>

namespace Fanex.MailDeliver.Model
{
    using System;

    [Serializable]
    public class MailError
    {
        public long ErrorID { get; set; }

        public long MailID { get; set; }

        public int AppID { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public string SettingName { get; set; }

        public DateTime ErrorTime { get; set; }
    }
}