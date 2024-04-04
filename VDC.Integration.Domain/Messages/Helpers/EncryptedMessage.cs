using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace VDC.Integration.Domain.Messages.Helpers
{
    public static class EncryptedMessage
    {
        public static string Compress(string body)
        {
            string result;

            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
                using (var writer = new BinaryWriter(gzip, Encoding.UTF8))
                {
                    writer.Write(body);
                }

                ms.Flush();
                result = Convert.ToBase64String(ms.ToArray());
            }

            return result;
        }

        public static string Decompress(string body)
        {
            string result;

            byte[] itemData = Convert.FromBase64String(body);
            using (MemoryStream src = new MemoryStream(itemData))
            using (GZipStream gzs = new GZipStream(src, CompressionMode.Decompress))
            using (var reader = new BinaryReader(gzs, Encoding.UTF8))
            {
                result = reader.ReadString();
            }

            return result;
        }
    }
}
