// --------------------------------------------------------------------------
// <copyright file="InsertMailErrorCriteria.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2013.11.11">Created</change>
// </history>

namespace Fanex.MailDeliver.Core.Criterias
{
    using Fanex.Data.Repository;
    using System;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class InsertMailErrorCriteria : CriteriaBase
    {
        public long MailID { get; set; }

        public int AppID { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public DateTime ErrorTime { get; set; }

        public int ErrorId { get; set; }

        public int ErrCode { get; set; }

        public string ErrorMsg { get; set; }

        public override string GetSettingKey()
        {
            return "MailRecorder_InsError";
        }

        public override bool IsValid()
        {
            return MailID > 0;
        }
    }
}