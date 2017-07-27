namespace TRuDI.HanAdapter.Interface
{
    using System;

    public class AdapterContext
    {
        /// <summary>
        /// The selected contract for readout.
        /// </summary>
        public ContractInfo Contract { get; set; }

        public BillingPeriod BillingPeriod { get; set; }

        /// <summary>
        /// Begin timestamp for device readout.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// End timestamp for device readout. 
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Only meaningful when TAF-7 is applicable.
        /// The AnalysisProfile may contain data that is neccessary for SMGW readout.
        /// </summary>
        public AnalysisProfile AnalysisProfile { get; set; }

        /// <summary>
        /// Log data can be huge and are probably not always required by the user.
        /// </summary>
        public bool WithLogdata { get; set; }
    }
}