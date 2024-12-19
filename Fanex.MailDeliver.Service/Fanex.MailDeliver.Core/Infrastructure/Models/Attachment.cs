using System.Collections.Generic;

namespace Fanex.MailDeliver.Core.Infrastructure.Models;

public class Attachment
{
    public AttachmentStoreMode StoreMode { get; set; }

    public List<FileType> Extensions = [];

    public string Path { get; set; }
}