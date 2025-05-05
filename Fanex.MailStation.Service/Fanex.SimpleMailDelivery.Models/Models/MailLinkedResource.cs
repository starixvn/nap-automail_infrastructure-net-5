namespace Fanex.SimpleMailDelivery.Models;

public class MailLinkedResource : MailAttachment
{
    public MailLinkedResource()
    { 
    }

    public MailLinkedResource(string filePath, FileExtension fileExtension, string contentId) 
        : base(filePath, fileExtension)
    {
        ContentId = contentId;
    }

    public MailLinkedResource(string fileName, byte[] fileContent, FileExtension fileExtension, string contentId)
        : base(fileName, fileContent, fileExtension)
    {
        ContentId = contentId;
    }

    public string ContentId { get; set; }
}