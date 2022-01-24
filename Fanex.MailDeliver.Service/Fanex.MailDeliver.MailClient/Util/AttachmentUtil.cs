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
    using System.IO;
    using Fanex.MailDeliver.Model;
    using Fanex.MailDeliver.Utility;

    /// <summary>
    /// Attachment Utility.
    /// </summary>
    public class AttachmentUtil
    {
        /// <summary>
        /// For match attachment before send mail.
        /// </summary>
        /// <param name="record">Mail Record contains information.</param>
        public static void Format(ref MailRecord record)
        {
            if (record.Attachments != null)
            {
                foreach (var attachment in record.Attachments)
                {
                    //if (attachment.Content == null && !string.IsNullOrEmpty(attachment.FileName))
                    //{
                    //    var stream = System.IO.File.Open(attachment.FileName, FileMode.Open);
                    //    var buffer = new byte[stream.Length];
                    //    stream.Read(buffer, 0, buffer.Length);
                    //    stream.Flush();
                    //    stream.Close();
                    //    attachment.Content = new MemoryStream(buffer);
                    //}

                    //attachment.FileName = Path.GetFileName(attachment.FileName);
                    attachment.Bytes = FileBinaryService.ConvertFileToBinary(attachment.FileName);
                }
            }

            if (record.AlternateView != null)
            {
                foreach (var resource in record.AlternateView)
                {
                    //if (resource.StreamContent == null && !string.IsNullOrEmpty(resource.FileName))
                    //{
                    //    var stream = System.IO.File.Open(resource.FileName, FileMode.Open);
                    //    var buffer = new byte[stream.Length];
                    //    stream.Read(buffer, 0, buffer.Length);
                    //    stream.Flush();
                    //    stream.Close();
                    //    resource.StreamContent = new MemoryStream(buffer);
                    //}

                    //resource.FileName = Path.GetFileName(resource.FileName);
                    resource.Bytes = FileBinaryService.ConvertFileToBinary(resource.FileName);
                }
            }
        }
    }
}