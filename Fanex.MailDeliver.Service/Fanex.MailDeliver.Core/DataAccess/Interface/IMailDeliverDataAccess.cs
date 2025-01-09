// --------------------------------------------------------------------------
// <copyright file="IMailDeliverDataAccess.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

using Fanex.MailDeliver.Core.Criterias;
using Fanex.MailDeliver.MailClient.Model;
using System;
using System.Collections.Generic;

namespace Fanex.MailDeliver.Core.DataAccess.Interface;

public interface IMailDeliverDataAccess
{
    long InsertMailRecord(MailRecord mailItem);

    MailConfig SaveMailConfig(MailConfig config);

    MailRecord GetMailRecord(long mailID, bool isSend);

    IEnumerable<MailRecord> GetListMailRecordByDate(int appID, DateTime mailDate, int page = 1, int counter = 100);

    IEnumerable<MailRecord> GetListMailRecordByDateRange(
        int appID,
        DateTime fromDate,
        DateTime toDate,
        ref int totalRows,
        int page = 1,
        int counter = 100,
        int status = 0);

    MailConfig GetMailConfig(int appID);

    MailSetting SaveMailSetting(MailSetting setting);

    MailSetting GetMailSetting(int settingID);

    MailFunction SaveMailFunction(MailFunction func);

    MailFunction GetMailFunction(int funcID);

    void UpdateMailStatus(long mailID, bool status);

    List<MailSetting> GetListMailSetting();

    List<MailConfig> GetListMailConfig(string serverIp = "");

    List<MailFunction> GetListMailFuntion();

    long InsertMailError(InsertMailErrorCriteria criteria);
}