// --------------------------------------------------------------------------
// <copyright file="MailFormatter.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.MailClient.Util
{
    using System.IO;
    using System.Text.Json;

    public class MailFormatter
    {
        public static byte[] Serialize(object obj)
        {
            var encypt = JsonSerializer.SerializeToUtf8Bytes(obj);
            return encypt;
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return (T)JsonSerializer.DeserializeAsync(stream, typeof(T)).Result;
            }
        }
    }
}