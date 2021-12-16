// --------------------------------------------------------------------------
// <copyright file="MemTable.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.Business.TableCaching
{
    using Fanex.MailDeliver.DataAccess.Implement;
    using Fanex.MailDeliver.DataAccess.Interface;

    /// <summary>
    /// Memory table.
    /// </summary>
    public class MemTable
    {
        public MemTable()
        {
            MailDeliverDataAccessInstance = new MailDeliverDataAccess();
        }

        protected IMailDeliverDataAccess MailDeliverDataAccessInstance { get; set; }

        public virtual void InitMem(string[] args)
        {
            ////TODO
        }

        public virtual void Set(object type)
        {
            ////TODO
        }

        public virtual object Get(int key)
        {
            return null;
        }
    }
}