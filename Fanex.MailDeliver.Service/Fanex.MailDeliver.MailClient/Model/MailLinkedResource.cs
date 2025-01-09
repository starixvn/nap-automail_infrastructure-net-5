// --------------------------------------------------------------------------
// <copyright file="MailLinkedResource.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

using System;

namespace Fanex.MailDeliver.MailClient.Model;

[Serializable]
public class MailLinkedResource
{
    private string __fileName = string.Empty;
    private FileExtentions __imageType;
    private string __contentID = string.Empty;

    public MailLinkedResource()
    {
    }

    public MailLinkedResource(string fileName, FileExtentions imageType, string contentID)
    {
        __fileName = fileName;
        __imageType = imageType;
        __contentID = contentID;
    }

    public MailLinkedResource(System.IO.MemoryStream streamContent, FileExtentions imageType, string contentID)
    {
        __imageType = imageType;
        __contentID = contentID;
        Bytes = streamContent.ToArray();
    }

    public byte[] Bytes { get; set; } = [];

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
        get => __fileName;

        set => __fileName = value;
    }

    public FileExtentions ImageType
    {
        get => __imageType;

        set => __imageType = value;
    }

    public string ContentCID
    {
        get => __contentID;

        set => __contentID = value;
    }
}