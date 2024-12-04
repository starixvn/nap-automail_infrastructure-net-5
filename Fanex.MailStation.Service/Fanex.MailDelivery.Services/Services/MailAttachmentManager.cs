using Fanex.Data;
using Fanex.MailDeliver.MailClient.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Fanex.MailDelivery.Services.Services;

public class MailAttachmentManager
{
    private long _mailID;

    public void StoreMailAttachments(
        long mailID,
        IEnumerable<MailAttachment> attachments)
    {
        _mailID = mailID;
        StoreMailAttachmentsToDatabase(mailID, attachments);
    }

    private void StoreMailAttachmentsToDatabase(
        long mailID,
        IEnumerable<MailAttachment> attachments)
    {
        if (mailID > 0 && attachments != null)
        {
            foreach (var attachment in attachments)
            {
                if (attachment.Bytes.Length > 0)
                {
                    var param = new Hashtable();
                    param.Add("mailid", mailID);
                    param.Add("filename", attachment.FileName);
                    param.Add("contenttype", attachment.ContentType);
                    param.Add("content", attachment.Bytes);
                    using var objDb = ObjectDbFactory.CreateInstance("StoreMailAttachement");

                    objDb.ExecuteNonQuery(param);
                }
            }
        }
    }

    public IEnumerable<MailAttachment> GetMailAttachments(long mailID)
    {
        return GetMailAttachmentsFromDatabase(mailID);
    }

    private IEnumerable<MailAttachment> GetMailAttachmentsFromDatabase(long mailID)
    {
        var attachments = new List<MailAttachment>();
        if (mailID > 0)
        {
            var param = new Hashtable();
            param.Add("mailid", mailID);
            using var objDb = ObjectDbFactory.CreateInstance("GetMailAttachement");
            var result = objDb.ExecuteDataSet(param);
            if (result.Tables.Count > 0)
            {
                var rows = result.Tables[0].Rows;
                foreach (DataRow row in rows)
                {
                    var item = new MailAttachment
                    {
                        MailID = _mailID,
                        FileName = Convert.ToString(row["filename"]),
                        ContentType = (FileExtentions)Enum.Parse(typeof(FileExtentions), Convert.ToString(row["contenttype"]), true)
                    };
                    byte[] binary = row["content"] as byte[];
                    item.Bytes = binary;
                    attachments.Add(item);
                }
            }
        }

        return attachments;
    }
}