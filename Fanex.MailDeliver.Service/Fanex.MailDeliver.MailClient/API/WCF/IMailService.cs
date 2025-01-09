using Fanex.MailDeliver.MailClient.Model;

namespace Fanex.MailDeliver.MailClient.API.WCF;

using ParamModel;
using System;
using System.Collections.Generic;

public interface IMailService
{
    void SendMail(MailRecord mailRecord);

    byte[] GetMail(long mailId);

    IEnumerable<byte[]> GetListMail(GetListMailParam param);

    Tuple<IEnumerable<byte[]>, int> GetListMailByDate(GetListMailByDateParam param);

    byte[] StoreConfig(MailConfig mailConfig);

    byte[] GetConfig(int appId);

    byte[] StoreSetting(MailSetting mailSetting);

    byte[] GetSetting(int settingID);

    bool ReSendMail(long mailID);

    byte[] StoreFunction(MailFunction mailFunction);

    byte[] GetFunction(int funcID);

    IEnumerable<byte[]> GetListSetting();

    IEnumerable<byte[]> GetListConfig(string serverIp);

    IEnumerable<byte[]> GetListFuntion();

    IEnumerable<byte[]> GetListError(long mailID);
}