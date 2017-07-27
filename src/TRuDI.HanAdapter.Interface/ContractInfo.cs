namespace TRuDI.HanAdapter.Interface
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Contains data about a tariff contract.
    /// </summary>
    public class ContractInfo
    {
        /// <summary>
        /// BSI TAF ID.
        /// </summary>
        public TafId TafId { get; set; }

        /// <summary>
        /// Logical name of the TAF profile.
        /// </summary>
        public string TafName { get; set; }

        /// <summary>
        /// Contract description if available.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The "Herstellerübergreifende Identifikationsnummer" (DIN 43863-5, e.g. "1XXX0012345678") of the meter associated with this billing period.
        /// </summary>
        public string MeterId { get; set; }

        /// <summary>
        /// Gets or sets the metering point identifier (Zählpunktbezeichnung).
        /// </summary>
        public string MeteringPointId { get; set; }

        /// <summary>
        /// Gets or sets the supplier identifier.
        /// </summary>
        public string SupplierId { get; set; }

        /// <summary>
        /// Gets or sets the consumer identifier.
        /// </summary>
        public string ConsumerId { get; set; }

        /// <summary>
        /// Begin of contract.
        /// </summary>
        public DateTime Begin { get; set; }

        /// <summary>
        /// End of contrac. <c>null</c> if still active.
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        /// Available billing periods for this contract. 
        /// </summary>
        public IReadOnlyList<BillingPeriod> BillingPeriods { get; set; }
    }
}