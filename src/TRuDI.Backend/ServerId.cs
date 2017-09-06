namespace TRuDI.Backend
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using TRuDI.HanAdapter.XmlValidation.Models;

    /// <summary>
    /// Identification number for measuring devices.
    /// </summary>
    public class ServerId
    {
        public string Id { get; }
        public string FlagId { get; }
        public byte ProductionBlock { get; }
        public uint Number { get; }
        public ObisMedium Medium { get; }

        public ServerId(string serverId)
        {
            if (string.IsNullOrWhiteSpace(serverId))
            {
                throw new ArgumentException("No valid server id specified", nameof(serverId));
            }

            serverId = serverId.ToUpperInvariant();
            var match = Regex.Match(serverId, "([0-9E]{1})([A-Z]{3})([0-9]{2})([0-9]{8})");
            if (match.Success)
            {
                this.Id = serverId;

                this.Medium = (ObisMedium)byte.Parse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                this.FlagId = match.Groups[2].Value;
                this.ProductionBlock = byte.Parse(match.Groups[3].Value);
                this.Number = uint.Parse(match.Groups[4].Value);
                return;
            }

            match = Regex.Match(serverId, "^0A0[0-9E]{1}[0-9A-F]{16}$");
            if (match.Success)
            {
                this.Medium = (ObisMedium)byte.Parse(serverId.Substring(3, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                this.FlagId = $"{(char)byte.Parse(serverId.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)}{(char)byte.Parse(serverId.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)}{(char)byte.Parse(serverId.Substring(8, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)}";
                this.ProductionBlock = byte.Parse(serverId.Substring(10, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                this.Number = uint.Parse(serverId.Substring(12), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return;
            }

            throw new ArgumentException("Unknown format of server id", nameof(serverId));
        }

        public override string ToString()
        {
            return $"{(byte)this.Medium:X1} {this.FlagId} {this.ProductionBlock:D2} {this.Number:D8}";
        }

        public string ToHexString()
        {
            return $"0A{(byte)this.Medium:X2}{(byte)this.FlagId[0]:X2}{(byte)this.FlagId[1]:X2}{(byte)this.FlagId[2]:X2}{this.ProductionBlock:X2}{this.Number:X8}";
        }
    }
}
