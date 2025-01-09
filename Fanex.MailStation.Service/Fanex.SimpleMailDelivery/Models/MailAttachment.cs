namespace Fanex.SimpleMailDelivery.Models;

using System.IO;

public class MailAttachment
{
    public MailAttachment()
    { 
    }

    public MailAttachment(string filePath, FileExtension fileExtension)
    {
        ReadFileContent(filePath);
        FileExtension = fileExtension;
    }

    public MailAttachment(string fileName, byte[] fileContent, FileExtension fileExtension)
    {
        FileName = fileName;
        FileContent = fileContent;
        FileExtension = fileExtension;
    }

    public int Id { get; set; }

    public long MailId { get; set; }

    public FileExtension FileExtension { get; set; }

    public byte[] FileContent { get; set; }

    public string FileName { get; set; }
     
    private void ReadFileContent(string filePath)
    {
        if (FileContent != null
            || string.IsNullOrEmpty(filePath))
        {
            return;
        }

        using var stream = File.Open(filePath, FileMode.Open);
        FileContent = new byte[stream.Length];

        stream.ReadExactly(FileContent, 0, FileContent.Length);

        FileName = Path.GetFileName(filePath);
    }
}