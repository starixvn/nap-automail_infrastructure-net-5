// --------------------------------------------------------------------------
// <copyright file="MailSetting.cs" company="Nexcel Solutions Vietnam">
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
    public class MailSetting
    {
        public int SettingID
        {
            get;
            set;
        }

        public string SettingName
        {
            get;
            set;
        }

        public string Server
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public bool IsAuth
        {
            get;
            set;
        }
    }
}