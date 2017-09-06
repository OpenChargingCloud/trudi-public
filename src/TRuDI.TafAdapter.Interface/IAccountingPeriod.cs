namespace TRuDI.TafAdapter.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IAccountingPeriod
    {
        DateTime Begin { get; }
        DateTime End { get; }

        IReadOnlyList<Reading> InitialReadings { get; }
        IReadOnlyList<IAccountingSection> AccountingSections { get; }
        IReadOnlyList<Register> SummaryRegister { get; }
    }

    
}
