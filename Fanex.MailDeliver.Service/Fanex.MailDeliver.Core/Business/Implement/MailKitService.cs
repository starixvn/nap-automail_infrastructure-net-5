using Fanex.MailDeliver.Model;
using MailKit.Net.Smtp;
using MimeKit;
using System.Linq;

namespace Fanex.MailDeliver.Core.Business.Implement
{
    public interface MailKitService
    {
        void Send(MailRecord mailRecord, MailSetting mailSetting, string mailFrom);
    }

    public class MailKitServiceImp : MailKitService
    {
        public void Send(MailRecord mailRecord, MailSetting mailSetting, string mailFrom)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mailRecord.Title, mailFrom));

            mailRecord.MailCc = string.IsNullOrEmpty(mailRecord.MailCc) ? string.Empty : mailRecord.MailCc;
            mailRecord.MailBcc = string.IsNullOrEmpty(mailRecord.MailBcc) ? string.Empty : mailRecord.MailBcc;

            var mailsTo = mailRecord.MailTo.Split(';').ToList();
            var mailsCc = mailRecord.MailCc.Split(';').ToList();
            var mailsBCc = mailRecord.MailBcc.Split(';').ToList();

            mailsTo.ForEach(mail =>
            {
                if (!string.IsNullOrEmpty(mail))
                {
                    message.To.Add(new MailboxAddress(string.Empty, mail));
                }
            });

            mailsCc.ForEach(mail =>
            {
                if (!string.IsNullOrEmpty(mail))
                {
                    message.Cc.Add(new MailboxAddress(string.Empty, mail));
                }
            });

            mailsBCc.ForEach(mail =>
            {
                if (!string.IsNullOrEmpty(mail))
                {
                    message.Bcc.Add(new MailboxAddress(string.Empty, mail));
                }
            });

            message.Subject = mailRecord.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = mailRecord.Body
            };
            message.Body = builder.ToMessageBody();

            using var mailclient = new SmtpClient();

            mailclient.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            mailclient.CheckCertificateRevocation = false;
            mailclient.Connect(mailSetting.Server, mailSetting.Port, mailSetting.IsAuth);
            mailclient.Authenticate(mailSetting.Username, mailSetting.Password);

            mailclient.Send(message);

            mailclient.Disconnect(true);
        }
    }
}