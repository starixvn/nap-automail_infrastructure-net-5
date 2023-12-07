using Fanex.Data;
using Fanex.MailDelivery.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Fanex.MailDelivery.Services.Services
{
    public class LinkedResourcesManager
    {
        private long _mailID;

        public void Insert(
           long mailID,
           IEnumerable<MailLinkedResource> resources)
        {
            StoreMailLinkResourcesToDatabase(mailID, resources);
        }

        private void StoreMailLinkResourcesToDatabase(
            long mailID,
            IEnumerable<MailLinkedResource> resources)
        {
            if (mailID > 0 && resources.Any())
            {
                foreach (var resource in resources)
                {
                    if (resource.FileContent != null)
                    {
                        var param = new Hashtable();
                        param.Add("mailid", mailID);
                        param.Add("filename", resource.FileName);
                        param.Add("imagetype", resource.FileExtension);
                        param.Add("contentcid", resource.ContentId);
                        param.Add("streamcontent", resource.FileContent);
                        using (ObjectDb objDb = new ObjectDb("StoreLinkedResource"))
                        {
                            objDb.ExecuteNonQuery(param);
                        }
                    }
                }
            }
        }

        public IEnumerable<MailLinkedResource> GetLinkedResources(long mailID)
        {
            return GetLinkedResourcesFromDatabase(mailID);
        }

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