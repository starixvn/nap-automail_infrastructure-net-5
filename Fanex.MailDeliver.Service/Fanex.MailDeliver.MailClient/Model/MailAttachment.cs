// --------------------------------------------------------------------------
// <copyright file="MailAttachment.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.Model
{
    using System;

    [Serializable]
    public enum FileExtentions
    {
        /// <summary>
        /// Excel file.
        /// </summary>
        MS_EXCEL,

        /// <summary>
        /// Excel file.
        /// </summary>
        MS_DOC,

        /// <summary>
        /// Excel file.
        /// </summary>
        IMG_JPG,

        /// <summary>
        /// Excel file.
        /// </summary>
        IMG_GIF,

        /// <summary>
        /// Excel file.
        /// </summary>
        IMG_PNG,

        /// <summary>
        /// Excel file.
        /// </summary>
        PDF,

        /// <summary>
        /// Excel file.
        /// </summary>
        TEXT
    }

    [Serializable]
    public class MailAttachment
    {
        private string _fileName = "noname";
        private FileExtentions __contentType;

        public MailAttachment()
        {
        }

        public MailAttachment(string fileName, FileExtentions contentType)
        {
            _fileName = fileName;
            __contentType = contentType;
        }

        public MailAttachment(System.IO.MemoryStream content, FileExtentions contentType)
        {
            __contentType = contentType;
            Bytes = content.ToArray();
            content.Dispose();
        }

        public byte[] Bytes { get; set; } = { };

        public int AttachmentID
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
                return _fileName;
            }

            set
            {
                _fileName = value;
            }
        }

        public FileExtentions ContentType
        {
            get
            {
                return __contentType;
            }

            set
            {
                __contentType = value;
            }
        }
    }
}