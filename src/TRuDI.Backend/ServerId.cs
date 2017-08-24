namespace TRuDI.HanAdapter.Interface
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Identification number for measuring devices.
    /// </summary>
    public class ServerId
    {
        public string Id { get; }
        public string FlagId { get; }
        public string ProductionBlock { get; }
        public string Number { get; }
        public string Medium { get; }

        public ServerId(string serverId)
        {
            if (string.IsNullOrWhiteSpace(serverId))
            {
                throw new ArgumentException("No valid server id specified", nameof(serverId));
            }

            serverId = serverId.ToUpperInvariant();
            var match = Regex.Match(serverId, "([0-9E]{1})([A-Z]{3})([0-9]{2})([0-9]{8})");
            if (!match.Success)
            {
                throw new ArgumentException("Unknown format of server id", nameof(serverId));
            }

            this.Id = serverId;

            this.FlagId = match.Groups[2].Value;
            this.ProductionBlock = match.Groups[3].Value;
            this.Number = match.Groups[4].Value;
        }

        public override string ToString()
        {
            return $"{this.Medium} {this.FlagId} {this.ProductionBlock} {this.Number}";
        }
    }
}
