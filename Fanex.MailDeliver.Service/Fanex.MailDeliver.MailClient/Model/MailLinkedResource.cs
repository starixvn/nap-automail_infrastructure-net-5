// --------------------------------------------------------------------------
// <copyright file="MailLinkedResource.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

using System;

namespace Fanex.MailDeliver.Model
{
    [Serializable]
    public class MailLinkedResource
    {
        private string __fileName = string.Empty;
        private FileExtentions __imageType;
        private string __contentID = string.Empty;
        private System.IO.MemoryStream __streamContent;

        public MailLinkedResource()
        {
            ////TODO
        }

        public MailLinkedResource(string fileName, FileExtentions imageType, string contentID)
        {
            ////TODO
            __fileName = fileName;
            __imageType = imageType;
            __contentID = contentID;
        }

        public MailLinkedResource(System.IO.MemoryStream streamContent, FileExtentions imageType, string contentID)
        {
            ////TODO
            __streamContent = streamContent;
            __imageType = imageType;
            __contentID = contentID;
        }

        public int LinkedID
        {
            get;
            set;
        }

        public long MailID
        {
            get;
            set;
        }

        public string FileName
        {
            get
            {
                return __fileName;
            }

            set
            {
                __fileName = value;
            }
        }

        public FileExtentions ImageType
        {
            get
            {
                return __imageType;
            }

            set
            {
                __imageType = value;
            }
        }

        public string ContentCID
        {
            get
            {
                return __contentID;
            }

            set
            {
                __contentID = value;
            }
        }

        public System.IO.MemoryStream StreamContent
        {
            get
            {
                return __streamContent;
            }

            set
            {
                __streamContent = value;
            }
        }
    }
}