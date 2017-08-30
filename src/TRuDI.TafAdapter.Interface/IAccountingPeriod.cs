namespace TRuDI.TafAdapter.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IAccountingPeriod
    {
        DateTime Begin { get; }
        DateTime End { get; }
        long? InitialReading { get; }

        IReadOnlyList<IAccountingDay> AccountingDays { get; }
        IReadOnlyList<Register> SummaryRegister { get; }
    }

    
}
