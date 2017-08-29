namespace TRuDI.TafAdapter.Interface
{
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

    /// <summary>
    /// Plugin interface for a TRuDI TAF adapters.
    /// </summary>
    public interface ITafAdapter
    {
        AccountingPeriod Calculate(UsagePointAdapterTRuDI device, UsagePointLieferant supplier);
    }
}