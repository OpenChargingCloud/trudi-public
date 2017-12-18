namespace TRuDI.Backend.Application
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    
    using TRuDI.Backend.Utils;

    /// <summary>
    /// Helper class used to calculate checksums of important application files.
    /// </summary>
    public class ApplicationChecksums
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationChecksums"/> class.
        /// </summary>
        public ApplicationChecksums()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.Items = new List<DigestItem>
                                 {
                                     this.GetDigest("../../../../../TRuDI.exe"),
                                     this.GetDigest("../../../../app.asar"),
                                     this.GetDigest("../../../../electron.asar"),
                                 };
            }
            else
            {
                this.Items = new List<DigestItem>
                                 {
                                     this.GetDigest("../../../../../trudi"),
                                     this.GetDigest("../../../../app.asar"),
                                     this.GetDigest("../../../../electron.asar"),
                                 };
            }
        }

        /// <summary>
        /// Gets the list of files with calculated digest values.
        /// </summary>
        public IReadOnlyList<DigestItem> Items { get; }

        /// <summary>
        /// Gets the digest of the specified file.
        /// </summary>
        /// <param name="filename">The absolute or relative path to the file.</param>
        /// <returns>A new instance of <see cref="DigestItem"/>.</returns>
        private DigestItem GetDigest(string filename)
        {
            var directory = Path.GetDirectoryName(this.GetType().Assembly.Location);
            if (File.Exists(Path.Combine(directory, filename)))
            {
                return new DigestItem { Filename = Path.GetFileName(filename), Digest = DigestUtils.GetRipemd160(filename) };
            }

            return new DigestItem { Filename = Path.GetFileName(filename), Digest = "nicht gefunden" };
        }

        /// <summary>
        /// Contains the filename and the digest value of the file.
        /// </summary>
        public class DigestItem
        {
            /// <summary>
            /// Gets or sets the filename (without path).
            /// </summary>
            public string Filename { get; set; }

            /// <summary>
            /// Gets or sets the digest value as hex formatted string.
            /// </summary>
            public string Digest { get; set; }
        }
    }
}
