// --------------------------------------------------------------------------
// <copyright file="MemTable.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

using Fanex.MailDeliver.Core.DataAccess.Implement;
using Fanex.MailDeliver.Core.DataAccess.Interface;

namespace Fanex.MailDeliver.Core.Business.TableCaching;

/// <summary>
/// Memory table.
/// </summary>
public class MemTable
{
    protected IMailDeliverDataAccess MailDeliverDataAccessInstance { get; set; } = new MailDeliverDataAccess();

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