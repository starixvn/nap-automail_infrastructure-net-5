// --------------------------------------------------------------------------
// <copyright file="MailFunction.cs" company="Nexcel Solutions Vietnam">
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
    public class MailFunction
    {
        public int FuncID
        {
            get;
            set;
        }

        public int AppID
        {
            get;
            set;
        }

        public MailConfig Config
        {
            get;
            set;
        }

        public string FuncName
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
    }
}