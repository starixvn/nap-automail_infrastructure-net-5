// --------------------------------------------------------------------------
// <copyright file="MailAttachmentManager.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.DataAccess.Implement
{
    using Fanex.Data;
    using Fanex.MailDeliver.Core.Infrastructure.Logging;
    using Fanex.MailDeliver.Core.Infrastructure.Models;
    using Fanex.MailDeliver.Core.Infrastructure.Services;
    using Fanex.MailDeliver.Model;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Storage Attachments.
    /// </summary>
    public class MailAttachmentManager
    {
        private long _mailID;

        /// <summary>
        /// Save Attachment.
        /// </summary>
        /// <param name="mailID">Mail record id.</param>
        /// <param name="attachments">List attachment files.</param>
        public void StoreMailAttachments(
            long mailID,
            IEnumerable<MailAttachment> attachments)
        {
            try
            {
                _mailID = mailID;
                var mailInfo = AppConfigService.Config.MailSection;
                switch (mailInfo.Attachment.StoreMode)
                {
                    case AttachmentStoreMode.Binary:
                        StoreMailAttchmentsToDatabase(mailID, attachments);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        /// <summary>
        /// Get attachment files.
        /// </summary>
        /// <param name="mailID">Mail record ID.</param>
        /// <param name="attInfo">Attachment Info.</param>
        /// <returns>List attachment files.</returns>
        public IEnumerable<MailAttachment> GetMailAttachments(
            long mailID,
            string attInfo)
        {
            try
            {
                _mailID = mailID;
                var mailInfo = AppConfigService.Config.MailSection;
                switch (mailInfo.Attachment.StoreMode)
                {
                    case AttachmentStoreMode.Binary:
                        return GetMailAttachmentsFromDatabase(mailInfo, mailID);

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Store attachment to Database.
        /// </summary>
        /// <param name="mailID">Mail record id.</param>
        /// <param name="attachments">List Files.</param>
        private void StoreMailAttchmentsToDatabase(
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
                        using (ObjectDb objDb = new ObjectDb("StoreMailAttachement"))
                        {
                            objDb.ExecuteNonQuery(param);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get List attachment files from Database.
        /// </summary>
        /// <param name="mailInfo">Mail information.</param>
        /// <param name="mailID">Mail record id.</param>
        /// <returns>List attachment mails.</returns>
        private IEnumerable<MailAttachment> GetMailAttachmentsFromDatabase(
            MailSection mailInfo,
            long mailID)
        {
            var attachments = new List<MailAttachment>();
            if (mailID > 0)
            {
                var param = new Hashtable();
                param.Add("mailid", mailID);
                using (ObjectDb objDb = new ObjectDb("GetMailAttachement"))
                {
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
            }

            return attachments;
        }
    }
}