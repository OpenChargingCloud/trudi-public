namespace TRuDI.Backend.Utils
{
    using System;
    using System.IO;
    using System.Text;

    using Org.BouncyCastle.Crypto.Digests;

    public static class DigestUtils
    {
        public static string GetDigestFromAssembly(Type type)
        {
            var digest = new RipeMD160Digest();
            var data = File.ReadAllBytes(type.Assembly.Location);

            digest.BlockUpdate(data, 0, data.Length);

            var result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);

            return BitConverter.ToString(result).Replace("-", "");
        }

        public static string GetSha3(MemoryStream data)
        {
            var digest = new KeccakDigest();
            digest.BlockUpdate(data.ToArray(), 0, (int)data.Length);

            var result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);

            return BitConverter.ToString(result).Replace("-", "");
        }

        public static string GetRipemd160(MemoryStream data)
        {
            var digest = new RipeMD160Digest();
            digest.BlockUpdate(data.ToArray(), 0, (int)data.Length);

            var result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);

            return BitConverter.ToString(result).Replace("-", "");
        }

        public static string GetRipemd160(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                byte[] buffer = new byte[4092];
                int bytesRead;

                var digest = new RipeMD160Digest();
                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    digest.BlockUpdate(buffer, 0, bytesRead);
                }

                var result = new byte[digest.GetDigestSize()];
                digest.DoFinal(result, 0);

                return BitConverter.ToString(result).Replace("-", "");
            }
        }
    }
}
