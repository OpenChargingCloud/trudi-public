namespace TRuDI.TafAdapter.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IAccountingDay
    {
        DateTime Date { get; }
        long? Reading { get; }

        IReadOnlyList<IMeasuringRange> MeasuringRanges { get; }
        IReadOnlyList<Register> SummaryRegister { get; }
    }


}
