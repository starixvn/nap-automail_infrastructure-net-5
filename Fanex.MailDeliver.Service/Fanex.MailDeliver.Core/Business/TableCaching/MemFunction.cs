// --------------------------------------------------------------------------
// <copyright file="MemFunction.cs" company="Nexcel Solutions Vietnam">
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
    /// Function cache.
    /// </summary>
    internal class MemFunction : MemTable
    {
        #region private

        private static readonly string Identify = "mail_func_{0}";
        private static Dictionary<int, MailFunction> _dictFunction = new Dictionary<int, MailFunction>();

        #endregion private

        public MemFunction()
            : base()
        {
        }

        /// <summary>
        /// Indexer class.
        /// </summary>
        /// <param name="funcID">Function ID.</param>
        /// <returns>Mail Function.</returns>
        public MailFunction this[int funcID]
        {
            get
            {
                if (_dictFunction.ContainsKey(funcID))
                {
                    return _dictFunction[funcID];
                }

                return null;
            }

            set
            {
                var key = string.Format(Identify, funcID);
                if (value == null)
                {
                    _dictFunction.Remove(funcID);
                }
                else
                {
                    _dictFunction[funcID] = value;
                }
            }
        }

        /// <summary>
        /// Initial List mail function to memory.
        /// </summary>
        /// <param name="args">Argument array.</param>
        public override void InitMem(string[] args)
        {
            try
            {
                var functions = MailDeliverDataAccessInstance.GetListMailFuntion();
                if (functions != null)
                {
                    foreach (var func in functions)
                    {
                        _dictFunction[func.FuncID] = func;
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        /// <summary>
        /// Set Mail function to cache.
        /// </summary>
        /// <param name="func">Function object.</param>
        public override void Set(object func)
        {
            var mailFunction = func as MailFunction;
            if (mailFunction != null && mailFunction.FuncID > 0)
            {
                _dictFunction[mailFunction.FuncID] = mailFunction;
            }
        }

        /// <summary>
        /// Get Mail function from cache.
        /// </summary>
        /// <param name="funcID">Function id.</param>
        /// <returns>Mail Function.</returns>
        public override object Get(int funcID)
        {
            object objData = null;
            if (_dictFunction.ContainsKey(funcID))
            {
                objData = _dictFunction[funcID];
            }

            return objData;
        }
    }
}