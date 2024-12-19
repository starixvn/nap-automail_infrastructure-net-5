// --------------------------------------------------------------------------
// <copyright file="GetMailErrorCriteria.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2013.8.5">Created</change>
// </history>

using Fanex.Data.Repository;

namespace Fanex.MailDeliver.Core.Criterias;

/// <summary>
/// TODO: Update summary.
/// </summary>
public class GetMailErrorCriteria : CriteriaBase
{
    public long MailID { get; set; }

    public override string GetSettingKey()
    {
        return "MailRecorder_GetError";
    }

    public override bool IsValid()
    {
        return MailID > 0;
    }
}