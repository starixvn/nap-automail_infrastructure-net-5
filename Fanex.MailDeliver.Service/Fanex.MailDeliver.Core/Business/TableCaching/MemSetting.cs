// --------------------------------------------------------------------------
// <copyright file="MemSetting.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

using Fanex.MailDeliver.Core.Infrastructure.Logging;
using Fanex.MailDeliver.MailClient.Model;
using System;
using System.Collections.Generic;

namespace Fanex.MailDeliver.Core.Business.TableCaching
{
    /// <summary>
    /// Mail Setting cache.
    /// </summary>
    public class MemSetting : MemTable
    {
        #region private

        private static readonly string Identify = "mail_setting_{0}";
        private static Dictionary<int, MailSetting> _dictSetting = new Dictionary<int, MailSetting>();

        #endregion private

        public MemSetting()
            : base()
        {
            ////TODO
        }

        /// <summary>
        /// Array Indexer.
        /// </summary>
        /// <param name="settingID">Mail setting id.</param>
        /// <returns>Mail setting information.</returns>
        public MailSetting this[int settingID]
        {
            get
            {
                if (_dictSetting.ContainsKey(settingID))
                {
                    return _dictSetting[settingID];
                }

                return null;
            }

            set
            {
                var key = string.Format(Identify, settingID.ToString());
                if (value == null)
                {
                    _dictSetting.Remove(settingID);
                }
                else
                {
                    _dictSetting[settingID] = value;
                }
            }
        }

        /// <summary>
        /// Get List Mail.
        /// </summary>
        /// <param name="args">Argument array.</param>
        public override void InitMem(string[] args)
        {
            try
            {
                var settings = MailDeliverDataAccessInstance.GetListMailSetting();
                if (settings != null)
                {
                    foreach (var setting in settings)
                    {
                        _dictSetting[setting.SettingID] = setting;
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        /// <summary>
        /// Set object to cache.
        /// </summary>
        /// <param name="setting">Setting data.</param>
        public override void Set(object setting)
        {
            var mailSetting = setting as MailSetting;
            if (mailSetting != null && mailSetting.SettingID > 0)
            {
                _dictSetting[mailSetting.SettingID] = mailSetting;
            }
        }

        /// <summary>
        /// Get setting object from cache.
        /// </summary>
        /// <param name="settingID">Setting id.</param>
        /// <returns>Setting data.</returns>
        public override object Get(int settingID)
        {
            object objData = null;
            if (_dictSetting.ContainsKey(settingID))
            {
                objData = _dictSetting[settingID];
            }

            return objData;
        }
    }
}