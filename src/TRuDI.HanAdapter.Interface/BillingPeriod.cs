namespace TRuDI.HanAdapter.Interface
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class BillingPeriod
    {
        /// <summary>
        /// Begin of period
        /// </summary>
        public DateTime Begin { get; set; }

        /// <summary>
        /// End of period (<c>null</c> if still active)
        /// </summary>
        public DateTime? End { get; set; }
    }
}