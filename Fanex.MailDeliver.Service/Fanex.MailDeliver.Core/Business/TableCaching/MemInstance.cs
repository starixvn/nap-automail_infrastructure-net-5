// --------------------------------------------------------------------------
// <copyright file="MemInstance.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.Business.TableCaching
{
    using System;
    using System.Reflection;

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
            MemTable[] memTables = new MemTable[]
            {
                new MemSetting(), new MemConfig(), new MemFunction()
            };
            Type type = typeof(MemTable);
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            MethodInfo method = type.GetMethod("InitMem", flags);
            if (method != null)
            {
                string[] args = new string[] { string.Empty };
                foreach (MemTable memTable in memTables)
                {
                    method.Invoke(memTable, new object[] { args });
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
            Type objType = typeof(MemTable);
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            MethodInfo method = objType.GetMethod("Set", flags);
            if (method != null)
            {
                method.Invoke(instance, new object[] { type });
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
            Type objType = typeof(MemTable);
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            MethodInfo method = objType.GetMethod("Get", flags);
            if (method != null)
            {
                result = method.Invoke(instance, new object[] { key });
            }

            return result;
        }
    }
}