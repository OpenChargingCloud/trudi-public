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
    }
}
