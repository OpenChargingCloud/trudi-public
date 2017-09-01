﻿namespace TRuDI.HanAdapter.XmlValidation.Models
{
    using System;
    using System.Globalization;

    public class ObisId
    {
        public ObisId()
        {
        }

        public ObisId(string hexString)
        {
            var value = ulong.Parse(hexString, NumberStyles.HexNumber);
            if (value > 0xFFFFFFFFFFFF)
            {
                throw new ArgumentException("OBIS id must be a value less or equal to 0xFFFFFFFFFFFF", nameof(hexString));
            }

            this.A = (byte)((value >> 40) & 0xFF);
            this.B = (byte)((value >> 32) & 0xFF);
            this.C = (byte)((value >> 24) & 0xFF);
            this.D = (byte)((value >> 16) & 0xFF);
            this.E = (byte)((value >> 8) & 0xFF);
            this.F = (byte)(value & 0xFF);
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