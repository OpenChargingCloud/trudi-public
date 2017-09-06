namespace TRuDI.HanAdapter.XmlValidation.Models
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public class ObisId
    {
        public ObisId()
        {
        }

        public ObisId(ObisId src)
        {
            this.A = src.A;
            this.B = src.B;
            this.C = src.C;
            this.D = src.D;
            this.E = src.E;
            this.F = src.F;
        }

        public ObisId(string hexString)
        {
            if (Regex.IsMatch(hexString, "[0-9A-Fa-f]{12}"))
            {
                var value = ulong.Parse(hexString, NumberStyles.HexNumber);
                if (value > 0xFFFFFFFFFFFF)
                {
                    throw new ArgumentException(
                        "OBIS id must be a value less or equal to 0xFFFFFFFFFFFF",
                        nameof(hexString));
                }

                this.A = (byte)((value >> 40) & 0xFF);
                this.B = (byte)((value >> 32) & 0xFF);
                this.C = (byte)((value >> 24) & 0xFF);
                this.D = (byte)((value >> 16) & 0xFF);
                this.E = (byte)((value >> 8) & 0xFF);
                this.F = (byte)(value & 0xFF);
                return;
            }

            var match = Regex.Match(hexString, @"^(?<A>\d+)-(?<B>\d+)\:(?<C>\d+)\.(?<D>\d+)\.(?<E>\d+)\*(?<F>\d+)$|^(?<A>\d+)-(?<B>\d+)\:(?<C>\d+)\.(?<D>\d+)\.(?<E>\d+)$|^(?<C>\d+)\.(?<D>\d+)\.(?<E>\d+)$");
            if (match.Success)
            {
                if (match.Groups["A"].Success)
                {
                    this.A = byte.Parse(match.Groups["A"].Value);
                }
                else
                {
                    this.A = 1;
                }

                if (match.Groups["B"].Success)
                {
                    this.B = byte.Parse(match.Groups["B"].Value);
                }
                else
                {
                    this.B = 0;
                }

                this.C = byte.Parse(match.Groups["C"].Value);
                this.D = byte.Parse(match.Groups["D"].Value);
                this.E = byte.Parse(match.Groups["E"].Value);

                if (match.Groups["F"].Success)
                {
                    this.F = byte.Parse(match.Groups["F"].Value);
                }
                else
                {
                    this.F = 255;
                }
            }
        }


        public ObisMedium Medium => (ObisMedium)this.A;

        public byte A { get; set; }
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte F { get; set; }

        public override string ToString()
        {
            return $"{this.A}-{this.B}:{this.C}.{this.D}.{this.E}*{this.F}";
        }

        public string ToHexString()
        {
            return $"{this.A:X2}{this.B:X2}{this.C:X2}{this.D:X2}{this.E:X2}{this.F:X2}";
        }
    }
}
