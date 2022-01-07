using Newtonsoft.Json;
using System.Text;

namespace Fanex.MailDeliver.Utility
{
    public static class MailFormatter
    {
        public static byte[] Serialize(object obj)
        {
            var encypt = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));

            return encypt;
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            string json = Encoding.UTF8.GetString(buffer);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}