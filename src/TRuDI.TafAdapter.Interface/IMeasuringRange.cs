namespace TRuDI.TafAdapter.Interface
{
    using System;

    public interface IMeasuringRange
    {
        DateTime Start { get; }
        DateTime End { get; }
        ushort TariffId { get; }
        long Amount { get; }
    }



   
}
