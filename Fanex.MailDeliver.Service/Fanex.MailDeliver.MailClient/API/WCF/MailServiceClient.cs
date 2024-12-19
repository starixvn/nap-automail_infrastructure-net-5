using Fanex.MailDeliver.MailClient.Model;

namespace Fanex.MailDeliver.MailClient.API.WCF;

using ParamModel;
using Utility;
using System;
using System.Collections.Generic;

public class MailServiceClient : IMailService
{
    public MailServiceClient()
    {
        WebApiClientService.AppMailAddress = AppSettings.AppMailAddress;
    }

    public void SendMail(MailRecord mailRecord)
    {
        var action = $"MailService/SendMail";
        WebApiClientService.PostAsync<bool, MailRecord>(action, mailRecord);
    }

    public byte[] GetMail(long mailId)
    {
        var action = $"MailService/GetMail";
        return WebApiClientService.PostAsync<byte[], long>(action, mailId);
    }

    public IEnumerable<byte[]> GetListMail(GetListMailParam param)
    {
        var action = $"MailService/GetListMail";
        return WebApiClientService.PostAsync<IEnumerable<byte[]>, GetListMailParam>(action, param);
    }

    public Tuple<IEnumerable<byte[]>, int> GetListMailByDate(GetListMailByDateParam param)
    {
        var action = $"MailService/GetListMailByDate";
        return WebApiClientService.PostAsync<Tuple<IEnumerable<byte[]>, int>, GetListMailByDateParam>(action, param);
    }

    public byte[] StoreConfig(MailConfig mailConfig)
    {
        var action = $"MailService/StoreConfig";
        return WebApiClientService.PostAsync<byte[], MailConfig>(action, mailConfig);
    }

    public byte[] GetConfig(int appId)
    {
        var action = $"MailService/GetConfig";
        return WebApiClientService.PostAsync<byte[], int>(action, appId);
    }

    public byte[] StoreSetting(MailSetting mailSetting)
    {
        var action = $"MailService/StoreSetting";
        return WebApiClientService.PostAsync<byte[], MailSetting>(action, mailSetting);
    }

    public byte[] GetSetting(int settingID)
    {
        var action = $"MailService/GetSetting";
        return WebApiClientService.PostAsync<byte[], int>(action, settingID);
    }

    public bool ReSendMail(long mailID)
    {
        var action = $"MailService/ReSendMail";
        return WebApiClientService.PostAsync<bool, long>(action, mailID);
    }

    public byte[] StoreFunction(MailFunction mailFunction)
    {
        var action = $"MailService/StoreFunction";
        return WebApiClientService.PostAsync<byte[], MailFunction>(action, mailFunction);
    }

    public byte[] GetFunction(int funcID)
    {
        var action = $"MailService/GetFunction";
        return WebApiClientService.PostAsync<byte[], int>(action, funcID);
    }

    public IEnumerable<byte[]> GetListSetting()
    {
        var action = $"MailService/GetListSetting";
        return WebApiClientService.PostAsync<IEnumerable<byte[]>, int>(action, 0);
    }

    public IEnumerable<byte[]> GetListConfig(string serverIp)
    {
        var action = $"MailService/GetListConfig";
        return WebApiClientService.PostAsync<IEnumerable<byte[]>, string>(action, serverIp);
    }

    public IEnumerable<byte[]> GetListFuntion()
    {
        var action = $"MailService/GetListFuntion";
        return WebApiClientService.PostAsync<IEnumerable<byte[]>, int>(action, 0);
    }

    public IEnumerable<byte[]> GetListError(long mailID)
    {
        var action = $"MailService/GetListError";
        return WebApiClientService.PostAsync<IEnumerable<byte[]>, long>(action, mailID);
    }
}