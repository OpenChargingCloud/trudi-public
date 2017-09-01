namespace TRuDI.TafAdapter.Interface
{
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

    /// <summary>
    /// Plugin interface for TRuDI TAF adapters.
    /// </summary>
    public interface ITafAdapter
    {
        /// <summary>
        /// Calculates the TAF based of the meter reading from the SMGW and the supplier TAF file.
        /// </summary>
        /// <param name="device">The meter reading from the device.</param>
        /// <param name="supplier">The supplier XML file.</param>
        /// <returns>The calculated accounting period.</returns>
        IAccountingPeriod Calculate(UsagePointAdapterTRuDI device, UsagePointLieferant supplier);
    }
}