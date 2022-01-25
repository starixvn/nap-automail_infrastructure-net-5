// --------------------------------------------------------------------------
// <copyright file="LinkedResourcesManager.cs" company="Nexcel Solutions Vietnam">
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
    /// Linked resource manager.
    /// </summary>
    public class LinkedResourcesManager
    {
        private long _mailID;

        /// <summary>
        /// Insert MailAttachment.
        /// </summary>
        /// <param name="mailID">Mail id for inserting.</param>
        /// <param name="resources">Attachments resource.</param>
        public void Insert(
            long mailID,
            IEnumerable<MailLinkedResource> resources)
        {
            try
            {
                _mailID = mailID;
                var mailInfo = AppConfigService.Config.MailSection;
                switch (mailInfo.Attachment.StoreMode)
                {
                    case AttachmentStoreMode.Binary:
                        StoreMailLinkResourcesToDatabase(mailID, resources);
                        break;

                    case AttachmentStoreMode.File:
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
        /// Get linked resource with mail id.
        /// </summary>
        /// <param name="mailID">Current mail need get linked resource.</param>
        /// <returns>Mail resource list.</returns>
        public IEnumerable<MailLinkedResource> GetLinkedResources(long mailID)
        {
            try
            {
                _mailID = mailID;
                var mailInfo = AppConfigService.Config.MailSection;
                switch (mailInfo.Attachment.StoreMode)
                {
                    case AttachmentStoreMode.Binary:
                        return GetLinkedResourcesFromDatabase(mailID);

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
        /// Send mail resource store in database.
        /// </summary>
        /// <param name="mailID">Current mail id.</param>
        /// <param name="resources">Resource list.</param>
        private void StoreMailLinkResourcesToDatabase(
            long mailID,
            IEnumerable<MailLinkedResource> resources)
        {
            if (mailID > 0 && resources.Any())
            {
                foreach (var resource in resources)
                {
                    if (resource.Bytes != null)
                    {
                        var param = new Hashtable();
                        param.Add("mailid", mailID);
                        param.Add("filename", resource.FileName);
                        param.Add("imagetype", resource.ImageType);
                        param.Add("contentcid", resource.ContentCID);
                        param.Add("streamcontent", resource.Bytes);
                        using (ObjectDb objDb = new ObjectDb("StoreLinkedResource"))
                        {
                            objDb.ExecuteNonQuery(param);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get linked resource from database.
        /// </summary>
        /// <param name="mailID">Mail id need get linked resource.</param>
        /// <returns>Linked resource list.</returns>
        private IEnumerable<MailLinkedResource> GetLinkedResourcesFromDatabase(long mailID)
        {
            var resources = new List<MailLinkedResource>();
            if (mailID > 0)
            {
                using (ObjectDb objDb = new ObjectDb("GetMailLinkedResource"))
                {
                    var param = new Hashtable();
                    param.Add("mailid", mailID);
                    var result = objDb.ExecuteDataSet(param);
                    if (result.Tables.Count > 0)
                    {
                        var rows = result.Tables[0].Rows;
                        foreach (DataRow row in rows)
                        {
                            var ext = Convert.ToString(row["imagetype"]);
                            var item = new MailLinkedResource
                            {
                                LinkedID = Convert.ToInt32(row["linkedid"]),
                                MailID = _mailID,
                                FileName = Convert.ToString(row["filename"]),
                                ContentCID = Convert.ToString(row["contentcid"]),
                                ImageType = (FileExtentions)Enum.Parse(typeof(FileExtentions), Convert.ToString(row["imagetype"]), true)
                            };

                            byte[] binary = row["streamcontent"] as byte[];

                            item.Bytes = binary;
                            resources.Add(item);
                        }
                    }
                }
            }

            return resources;
        }
    }
}