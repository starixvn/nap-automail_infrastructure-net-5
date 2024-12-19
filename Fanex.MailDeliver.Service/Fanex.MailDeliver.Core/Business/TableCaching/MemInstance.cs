// --------------------------------------------------------------------------
// <copyright file="MemInstance.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

using System;
using System.Reflection;

namespace Fanex.MailDeliver.Core.Business.TableCaching;

/// <summary>
/// Memory cache instance.
/// </summary>
public class MemInstance
{
    /// <summary>
    /// Initialize memory cache.
    /// </summary>
    public static void Initialize()
    {
        var memTables = new MemTable[]
        {
            new MemSetting(), new MemConfig(), new MemFunction()
        };
        var type = typeof(MemTable);
        var flags = BindingFlags.Instance | BindingFlags.Public;
        var method = type.GetMethod("InitMem", flags);
        if (method != null)
        {
            var args = new[] { string.Empty };
            foreach (var memTable in memTables)
            {
                method.Invoke(memTable, [args]);
            }
        }
    }

    /// <summary>
    /// Set Object to cache, object can be Mail Function, Mail Config or Mail setting.
    /// </summary>
    /// <typeparam name="T">Object Type return.</typeparam>
    /// <param name="type">Type of object need to be invoke.</param>
    public static void Set<T>(object type)
    {
        var instance = Activator.CreateInstance<T>();
        var objType = typeof(MemTable);
        var flags = BindingFlags.Instance | BindingFlags.Public;
        var method = objType.GetMethod("Set", flags);
        if (method != null)
        {
            method.Invoke(instance, [type]);
        }
    }

    /// <summary>
    /// Get Object with Key.
    /// </summary>
    /// <typeparam name="T">Object type return.</typeparam>
    /// <param name="key">Key to get Object.</param>
    /// <returns>Object from cache.</returns>
    public static object Get<T>(int key)
    {
        object result = null;
        var instance = Activator.CreateInstance<T>();
        var objType = typeof(MemTable);
        var flags = BindingFlags.Instance | BindingFlags.Public;
        var method = objType.GetMethod("Get", flags);
        if (method != null)
        {
            result = method.Invoke(instance, [key]);
        }

        return result;
    }
}