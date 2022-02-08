// --------------------------------------------------------------------------
// <copyright file="AttachmentUtil.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.MailClient.Util
{
    using Fanex.MailDeliver.Model;
    using Fanex.MailDeliver.Utility;

    public class AttachmentUtil
    {
        public static void Format(ref MailRecord record)
        {
            if (record.Attachments != null)
            {
                foreach (var attachment in record.Attachments)
                {
                    if (attachment.Bytes.Length == 0)
                    {
                        attachment.Bytes = FileBinaryService.ConvertFileToBinary(attachment.FileName);
                    }
                }
            }

            if (record.AlternateView != null)
            {
                foreach (var resource in record.AlternateView)
                {
                    if (resource.Bytes.Length == 0)
                    {
                        resource.Bytes = FileBinaryService.ConvertFileToBinary(resource.FileName);
                    }
                }
            }
        }
    }
}