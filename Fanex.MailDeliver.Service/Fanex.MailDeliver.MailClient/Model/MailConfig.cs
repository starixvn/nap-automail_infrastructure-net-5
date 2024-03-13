// --------------------------------------------------------------------------
// <copyright file="MailConfig.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

using System;

namespace Fanex.MailDeliver.MailClient.Model
{
    [Serializable]
    public class MailConfig
    {
        public int AppID
        {
            get;
            set;
        }

        public string AppName
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public MailSetting Setting
        {
            get;
            set;
        }

        public int SettingID
        {
            get;
            set;
        }

        public string MailFrom
        {
            get;
            set;
        }

        public bool IsEncrypt
        {
            get;
            set;
        }

        public int UserId { get; set; }
    }
}