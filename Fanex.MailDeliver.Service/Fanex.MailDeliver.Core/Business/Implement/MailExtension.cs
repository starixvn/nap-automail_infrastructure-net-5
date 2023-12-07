// --------------------------------------------------------------------------
// <copyright file="MailExtension.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

using EASendMail;
using EasyNetQ;
using Fanex.MailDeliver.Core.Criterias;
using Fanex.MailDeliver.Core.DataAccess.Implement;
using Fanex.MailDeliver.Core.Infrastructure;
using Fanex.MailDeliver.Core.Infrastructure.Logging;
using Fanex.MailDeliver.Core.Infrastructure.Services;
using Fanex.MailDeliver.MailClient.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;

namespace Fanex.MailDeliver.Core.Business.Implement
{
    public interface MailExtension
    {
        void SendMail(MailRecord mailSend, out string message);

        void AddMailToQueueForSendingMail(MailRecord mailRecord);
    }

    public class MailExtensionImp : MailExtension
    {
        private readonly DataService dataService;
        private readonly MailKitService mailKitService;
        private const int ThresholdCounter = 14;
        private const int ThresholdSecond = 1;

        public MailExtensionImp(DataService dataService, MailKitService mailKitService)
        {
            this.dataService = dataService;
            this.mailKitService = mailKitService;
        }

        public void SendMail(MailRecord mailSend, out string message)
        {
            message = string.Empty;
            try
            {
                int mailConfigId = mailSend.Config != null ? mailSend.Config.AppID : mailSend.AppID;
                var mailSetting = GetMailSettingForSendingMail(
                    mailConfigId,
                    mailSend.Function != null ? mailSend.Function.FuncID : mailSend.FuncID);

                if (mailSetting == null)
                {
                    LogExtension.LogError(new Exception(string.Format("Application's setting \"{0}\" which was not supported.", mailSend.Config.AppID)));
                }

                if (mailSetting.Port == Constant.Port465)
                {
                    SendMailThroughImplicitSSL(mailConfigId, mailSetting, mailSend);
                }
                else
                {
                    var msg = new MailMessage();
                    var mailFrom = GetMailFrom(mailConfigId, mailSetting, mailSend);

                    msg.From = new System.Net.Mail.MailAddress(mailFrom, mailSend.Title, Encoding.UTF8);
                    if (!string.IsNullOrEmpty(mailSend.MailTo))
                    {
                        mailSend.MailTo = RemoveDoubleComma(mailSend.MailTo);
                        mailSend.MailTo.Split(';').ToList().ForEach(toMail =>
                        {
                            if (!string.IsNullOrEmpty(toMail))
                            {
                                msg.To.Add(toMail);
                            }
                        });
                    }
                    else
                    {
                        message = LogMailError(mailSend, new Exception("Mail To cannot be null or empty."));
                        return;
                    }

                    if (!string.IsNullOrEmpty(mailSend.MailCc))
                    {
                        mailSend.MailCc = RemoveDoubleComma(mailSend.MailCc);
                        mailSend.MailCc.Split(';').ToList().ForEach(ccMail =>
                        {
                            if (!string.IsNullOrEmpty(ccMail))
                            {
                                msg.CC.Add(ccMail);
                            }
                        });
                    }

                    if (!string.IsNullOrEmpty(mailSend.MailBcc))
                    {
                        mailSend.MailBcc = RemoveDoubleComma(mailSend.MailBcc);
                        mailSend.MailBcc.Split(';').ToList().ForEach(bccMail =>
                            {
                                if (!string.IsNullOrEmpty(bccMail))
                                {
                                    msg.Bcc.Add(bccMail);
                                }
                            });
                    }

                    msg.Subject = mailSend.Subject;
                    msg.SubjectEncoding = Encoding.UTF8;
                    msg.IsBodyHtml = true;
                    msg.Body = mailSend.Body;
                    msg.BodyEncoding = Encoding.UTF8;
                    msg.Priority = mailSend.Priority;

                    var attInfo = AppConfigService.Config.MailSection.Attachment;
                    var attachments = mailSend.Attachments;
                    if (attachments != null)
                    {
                        foreach (var attachFile in attachments)
                        {
                            var contType = attInfo.Extensions.FirstOrDefault(s => s.Name == attachFile.ContentType.ToString()).Type;
                            var content = new MemoryStream(attachFile.Bytes);
                            content.Seek(0, SeekOrigin.Begin);
                            var attachment = new System.Net.Mail.Attachment(content, attachFile.FileName, contType);
                            msg.Attachments.Add(attachment);
                        }
                    }

                    var alternateView = mailSend.AlternateView;
                    if (alternateView != null)
                    {
                        AlternateView altView = AlternateView.CreateAlternateViewFromString(msg.Body, Encoding.UTF8, MediaTypeNames.Text.Html);
                        foreach (var resoure in alternateView)
                        {
                            var contType = attInfo.Extensions.FirstOrDefault(s => s.Name == resoure.ImageType.ToString()).Type;
                            var content = new MemoryStream(resoure.Bytes);
                            content.Seek(0, SeekOrigin.Begin);

                            var linkedImage = new LinkedResource(content, contType);
                            linkedImage.ContentId = resoure.ContentCID;
                            linkedImage.ContentStream.Seek(0, SeekOrigin.Begin);
                            altView.LinkedResources.Add(linkedImage);
                        }

                        msg.AlternateViews.Add(altView);
                    }

                    var client = new System.Net.Mail.SmtpClient
                    {
                        Port = mailSetting.Port,
                        Host = mailSetting.Server
                    };
                    if (mailSetting.IsAuth)
                    {
                        client.Credentials = new System.Net.NetworkCredential(mailSetting.Username, mailSetting.Password);
                        client.EnableSsl = true;
                    }

                    System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;

                    SafeSmtpClientSendMessage(client, msg, mailSend, mailSetting, mailFrom);
                }
            }
            catch (Exception ex)
            {
                message = LogMailError(mailSend, ex);
            }
        }

        private void SafeSmtpClientSendMessage(System.Net.Mail.SmtpClient smtpClient, MailMessage msg, MailRecord mailRecord, MailSetting mailSetting, string mailFrom)
        {
            try
            {
                CheckTimeCounter();
                smtpClient.Send(msg);
                UpdateMailStatus(mailRecord, true, null);
            }
            catch (Exception e)
            {
                LogExtension.LogError(e);

                try
                {
                    mailKitService.Send(mailRecord, mailSetting, mailFrom);
                    UpdateMailStatus(mailRecord, true, null);
                }
                catch (Exception ex)
                {
                    LogExtension.LogError(ex);

                    UpdateMailStatus(mailRecord, false, e);
                }
            }
            finally
            {
                try
                {
                    smtpClient.Dispose();
                    msg.Dispose();
                }
                catch (Exception e)
                {
                    LogExtension.LogError(e);
                }
            }
        }

        private static void CheckTimeCounter()
        {
            var now = DateTime.Now;
            var timeToRemove = new List<DateTime>();

            foreach (DateTime time in AppConfigService.TimeCounter)
            {
                var timeDiff = now.Subtract(time).Seconds;
                if (timeDiff > ThresholdSecond)
                {
                    timeToRemove.Add(time);
                }
            }

            foreach (var time in timeToRemove)
            {
                AppConfigService.TimeCounter.Remove(time);
            }

            AppConfigService.TimeCounter.Add(now);
            var count = AppConfigService.TimeCounter.Count;
            if (count > ThresholdCounter)
            {
                Thread.Sleep(2000);
                LogExtension.LogMessage(string.Format($"Delay in 2 seconds, {JsonConvert.SerializeObject(AppConfigService.TimeCounter, Formatting.Indented)}"));
            }
        }

        public void AddMailToQueueForSendingMail(MailRecord mailRecord)
        {
            if (mailRecord != null)
            {
                try
                {
                    LogExtension.LogMessage(string.Format("\r\nPublish Email into RabbitMQ: {0}", mailRecord.Subject));
                    AppConfigService.Bus.PubSub.Publish(mailRecord);
                    if (!AppConfigService.Config.IsUsingRabbitMq)
                    {
                        mailRecord.MailID = dataService.InsertMailRecord(mailRecord);
                        mailRecord.IsReady = true;
                        var outputMessage = string.Empty;
                        SendMail(mailRecord, out outputMessage);
                    }
                }
                catch (Exception ex)
                {
                    LogExtension.LogMessage(ex.Message + "\n[Exception from RabbitMQ]Mail Subject: " + mailRecord.Subject);
                    mailRecord.MailID = dataService.InsertMailRecord(mailRecord);
                    mailRecord.IsReady = true;
                    var outputMessage = string.Empty;
                    SendMail(mailRecord, out outputMessage);
                }
            }
        }

        private string LogMailError(MailRecord mail, Exception ex)
        {
            LogExtension.LogError(ex);
            UpdateMailStatus(mail, false, ex);
            return ex.Message;
        }

        private MailSetting GetMailSettingForSendingMail(int appID, int funcID)
        {
            if (appID > 0)
            {
                int settingID = 0;
                var mailManager = new MailDeliverDataAccess();
                //// Check mail function
                if (funcID > 0)
                {
                    var funcInfo = mailManager.GetMailFunction(funcID);
                    if (funcInfo != null && funcInfo.Config.AppID == appID)
                    {
                        settingID = funcInfo.Setting.SettingID;
                    }
                }
                //// Check app mail
                if (settingID == 0)
                {
                    var appConfig = mailManager.GetMailConfig(appID);
                    if (appConfig != null)
                    {
                        settingID = appConfig.Setting.SettingID;
                    }
                }
                //// Get mai setting
                if (settingID > 0)
                {
                    var mailSetting = mailManager.GetMailSetting(settingID);
                    return mailSetting;
                }
            }

            return null;
        }

        private void UpdateMailStatus(MailRecord mailRecord, bool status, Exception error)
        {
            if (mailRecord.MailID > 0)
            {
                try
                {
                    dataService.UpdateMailStatus(mailRecord.MailID, status);

                    //// Resend mail if retry = true and resentcount > 0
                    if (status == false && mailRecord.ResentCount > 0)
                    {
                        lock (AppConfigService.ListMailLockObject)
                        {
                            AppConfigService.ListMailFailed.Add(mailRecord);
                        }
                    }

                    if (error != null)
                    {
                        var currentAppId = mailRecord.AppID > 0 ? mailRecord.AppID : (mailRecord.Config != null ? mailRecord.Config.AppID : 0);
                        dataService.InsertMailError(new InsertMailErrorCriteria
                        {
                            AppID = currentAppId,
                            ErrorTime = DateTime.Now,
                            MailID = mailRecord.MailID,
                            Message = error.Message,
                            StackTrace = error.ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogExtension.LogError(ex);
                }
            }
        }

        /// <summary>
        /// Remove double comma in email address.
        /// </summary>
        /// <param name="mails">Mail list string.</param>
        /// <returns>Mail list with only one comma.</returns>
        private string RemoveDoubleComma(string mails)
        {
            while (mails.Contains(",,"))
            {
                mails = mails.Replace(",,", ",");
            }

            while (mails.Contains(";;"))
            {
                mails = mails.Replace(";;", ";");
            }

            return mails.TrimEnd(new char[] { ',', ';' });
        }

        /// <summary>
        /// Send mail through implicit SSL.
        /// </summary>
        /// <param name="mailConfigId">Mail config.</param>
        /// <param name="mailSetting">Mail setting.</param>
        /// <param name="mailRecord">Mail information.</param>
        [SuppressMessage("StyleCop.CSharp.StylecopPlus.Rules", "SP2101:MethodMustNotContainMoreLinesThan", Justification = "Reviewed. Suppression is OK here.")]
        private void SendMailThroughImplicitSSL(int mailConfigId, MailSetting mailSetting, MailRecord mailRecord)
        {
            SmtpMail oMail = new SmtpMail("TryIt");
            EASendMail.SmtpClient oSmtp = new EASendMail.SmtpClient();

            // Set sender email address, please change it to yours
            oMail.From = GetMailFrom(mailConfigId, mailSetting, mailRecord);

            // Set recipient email address, please change it to yours
            oMail.To = mailRecord.MailTo;

            if (!string.IsNullOrEmpty(mailRecord.MailCc))
            {
                mailRecord.MailCc = RemoveDoubleComma(mailRecord.MailCc);
                mailRecord.MailCc.Split(';').ToList().ForEach(ccMail =>
                {
                    if (!string.IsNullOrEmpty(ccMail))
                    {
                        oMail.Cc.Add(ccMail);
                    }
                });
            }

            if (!string.IsNullOrEmpty(mailRecord.MailBcc))
            {
                mailRecord.MailBcc = RemoveDoubleComma(mailRecord.MailBcc);
                mailRecord.MailBcc.Split(';').ToList().ForEach(bccMail =>
                {
                    if (!string.IsNullOrEmpty(bccMail))
                    {
                        oMail.Bcc.Add(bccMail);
                    }
                });
            }

            // Set email subject
            oMail.Subject = mailRecord.Subject;

            // Set email body
            oMail.HtmlBody = mailRecord.Body;

            // Your SMTP server address
            SmtpServer oServer = new SmtpServer(mailSetting.Server);

            // User and password for ESMTP authentication, if your server doesn't require
            // User authentication, please remove the following codes.
            oServer.User = mailSetting.Username;
            oServer.Password = mailSetting.Password;

            // Set 465 SMTP port
            oServer.Port = mailSetting.Port;

            // Enable SSL connection
            oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

            try
            {
                oSmtp.SendMail(oServer, oMail);
                UpdateMailStatus(mailRecord, true, null);
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
                UpdateMailStatus(mailRecord, false, ex);
            }
        }

        /// <summary>
        /// Get mail from mail config.
        /// </summary>
        /// <param name="mailConfigId">Mail config id.</param>
        /// <param name="mailSetting">Mail setting.</param>
        /// <param name="mailSend">Mail information for send.</param>
        /// <returns>Mail from information.</returns>
        private string GetMailFrom(int mailConfigId, MailSetting mailSetting, MailRecord mailSend)
        {
            var mailFrom = mailSetting.Username;
            if (!string.IsNullOrEmpty(mailSend.MailFrom))
            {
                mailFrom = mailSend.MailFrom;
            }

            if (mailConfigId > 0)
            {
                var mailManager = new MailDeliverDataAccess();
                var appConfig = mailManager.GetMailConfig(mailConfigId);
                if (appConfig != null && !string.IsNullOrEmpty(appConfig.MailFrom))
                {
                    mailFrom = appConfig.MailFrom;
                }
            }

            return mailFrom;
        }
    }
}