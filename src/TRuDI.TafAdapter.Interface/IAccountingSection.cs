namespace TRuDI.TafAdapter.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IAccountingSection
    {
        DateTime Start { get; }
        Reading Reading { get; }

        IReadOnlyList<IMeasuringRange> MeasuringRanges { get; }
        IReadOnlyList<Register> SummaryRegister { get; }
    }


}
