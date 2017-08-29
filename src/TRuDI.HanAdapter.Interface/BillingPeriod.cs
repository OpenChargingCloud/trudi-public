namespace TRuDI.HanAdapter.Interface
{
    using System;

    /// <summary>
    /// Represents a timerange of a single billing period.
    /// </summary>
    public class BillingPeriod
    {
        /// <summary>
        /// Begin of the billing period.
        /// </summary>
        public DateTime Begin { get; set; }

        /// <summary>
        /// End of the billing period. <c>null</c> if there isn't a end defined.
        /// </summary>
        public DateTime? End { get; set; }
    }
}