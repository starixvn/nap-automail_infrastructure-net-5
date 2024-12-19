using Fanex.MailDeliver.MailClient.Model;
using MimeKit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Fanex.MailDeliver.Core.Business.Implement;

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
        var attachments = new List<MimePart>();
        if (mailRecord.Attachments != null)
        {
            mailRecord.Attachments.ForEach(att =>
            {
                var attachment = new MimePart
                {
                    Content = new MimeContent(new MemoryStream(att.Bytes)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = att.FileName
                };
                attachments.Add(attachment);
            });
        }

        var builder = new BodyBuilder
        {
            HtmlBody = mailRecord.Body
        };

        var multipart = new Multipart("mixed");
        multipart.Add(builder.ToMessageBody());
        attachments.ForEach(att =>
        {
            multipart.Add(att);
        });
        message.Body = multipart;

        using var mailClient = new SmtpClient();

        mailClient.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
        mailClient.CheckCertificateRevocation = false;

        mailClient.Connect(mailSetting.Server, mailSetting.Port, mailSetting.IsAuth);
        mailClient.Authenticate(mailSetting.Username, mailSetting.Password);

        mailClient.Send(message);

        mailClient.Disconnect(true);
    }
}