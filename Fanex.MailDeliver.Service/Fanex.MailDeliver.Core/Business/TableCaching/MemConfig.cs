// --------------------------------------------------------------------------
// <copyright file="MemConfig.cs" company="Nexcel Solutions Vietnam">
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

namespace Fanex.MailDeliver.Core.Business.TableCaching;

/// <summary>
/// Memory cache information.
/// </summary>
public class MemConfig : MemTable
{
    #region private

    private static readonly string Identify = "mail_config_{0}";
    private static Dictionary<int, MailConfig> _dictConfig = new();

    #endregion private

    public MemConfig()
    {
    }

    public MailConfig this[int appID]
    {
        get
        {
            if (_dictConfig.ContainsKey(appID))
            {
                return _dictConfig[appID];
            }

            return null;
        }

        set
        {
            var key = string.Format(Identify, appID);
            if (value == null)
            {
                _dictConfig.Remove(appID);
            }
            else
            {
                _dictConfig[appID] = value;
            }
        }
    }

    /// <summary>
    /// Get all MailConfig and add to memory.
    /// </summary>
    /// <param name="args">Argument list.</param>
    public override void InitMem(string[] args)
    {
        try
        {
            var configs = MailDeliverDataAccessInstance.GetListMailConfig();
            if (configs != null)
            {
                foreach (var config in configs)
                {
                    _dictConfig[config.AppID] = config;
                }
            }
        }
        catch (Exception ex)
        {
            LogExtension.LogError(ex);
        }
    }

    /// <summary>
    /// Set MailConfig to memory.
    /// </summary>
    /// <param name="config">Config object.</param>
    public override void Set(object config)
    {
        var mailConfig = config as MailConfig;
        if (config != null && mailConfig.AppID > 0)
        {
            _dictConfig[mailConfig.AppID] = mailConfig;
        }
    }

    /// <summary>
    /// Get Mail config from memory.
    /// </summary>
    /// <param name="appID">Application id.</param>
    /// <returns>Mail Config.</returns>
    public override object Get(int appID)
    {
        object objData = null;
        if (_dictConfig.ContainsKey(appID))
        {
            objData = _dictConfig[appID];
        }

        return objData;
    }
}