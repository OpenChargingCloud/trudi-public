namespace TRuDI.Backend.Application
{
    using System.Collections.Generic;
    using System.IO;

    using TRuDI.Backend.Utils;

    public class ApplicationChecksums
    {
        public IReadOnlyList<DigestItem> Items { get; }

        public ApplicationChecksums()
        {
            this.Items = new List<DigestItem>
                             {
                                 this.GetDigest("../../../../../TRuDI.exe"),
                                 this.GetDigest("../../../../app.asar"),
                                 this.GetDigest("../../../../electron.asar"),
                             };
        }

        private DigestItem GetDigest(string filename)
        {
            var directory = Path.GetDirectoryName(this.GetType().Assembly.Location);
            if (File.Exists(Path.Combine(directory, filename)))
            {
                return new DigestItem { Filename = Path.GetFileName(filename), Digest = DigestUtils.GetRipemd160(filename) };
            }

            return new DigestItem { Filename = Path.GetFileName(filename), Digest = "nicht gefunden" };
        }

        public class DigestItem
        {
            public string Filename { get; set; }
            public string Digest { get; set; }
        }

    }
}
