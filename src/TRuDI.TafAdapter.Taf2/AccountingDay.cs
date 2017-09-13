namespace TRuDI.TafAdapter.Taf2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TRuDI.Models;
    using TRuDI.TafAdapter.Interface;

    public class AccountingDay : IAccountingSection
    {
        private List<MeasuringRange> measuringRanges = new List<MeasuringRange>();
        private List<Register> summaryRegister;

        public AccountingDay(IList<Register> register)
        {
            this.summaryRegister = new List<Register>(register);
        }

        public DateTime Start { get; set; }

        public Reading Reading { get; set; }

        public void Add(MeasuringRange range, ObisId obisId)
        {
            this.measuringRanges.Add(range);
            this.summaryRegister.FirstOrDefault(r => r.TariffId == range.TariffId && obisId.C == r.ObisCode.C).Amount =
                summaryRegister.FirstOrDefault(r => r.TariffId == range.TariffId && obisId.C == r.ObisCode.C).Amount + range.Amount;  
        }

        public IReadOnlyList<IMeasuringRange> MeasuringRanges => this.measuringRanges;

        public IReadOnlyList<Register> SummaryRegister => this.summaryRegister;
    }
}
