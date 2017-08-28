namespace TRuDI.TafAdapter.Interface
{
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

    /// <summary>
    /// Plugin interface for a TRuDI TAF adapters.
    /// </summary>
    public interface ITafAdapter
    {
        object Calculate(UsagePointAdapterTRuDI device, UsagePointLieferant supplier);
    }
}