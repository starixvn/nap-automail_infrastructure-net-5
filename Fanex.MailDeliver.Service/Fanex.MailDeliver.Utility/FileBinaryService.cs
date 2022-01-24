using System.IO;

namespace Fanex.MailDeliver.Utility
{
    public static class FileBinaryService
    {
        public static byte[] ConvertFileToBinary(string fileName)
        {
            return File.ReadAllBytes(fileName);
        }

        public static void ConvertBinaryToFile(string filename, byte[] bytes)
        {
            File.WriteAllBytes(filename, bytes);
        }
    }
}