using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TRuDI.TafAdapter.Interface;

namespace TRuDI.TafAdapter.Taf1
{
    public class AccountingMonth : IAccountingSection
    {
        private List<MeasuringRange> measuringRanges = new List<MeasuringRange>();
        private List<Register> summaryRegister;

        public AccountingMonth(IList<Register> register)
        {
            this.summaryRegister = new List<Register>(register);
        }

        public DateTime Start
        {
            get; set;
        }

        public Reading Reading
        {
            get; set;
        }

        public void Add(MeasuringRange range)
        {
            this.measuringRanges.Add(range);

            this.summaryRegister.FirstOrDefault(r => r.TariffId == range.TariffId).Amount =
                       summaryRegister.FirstOrDefault(r => r.TariffId == range.TariffId).Amount + range.Amount;
        }

        public IReadOnlyList<IMeasuringRange> MeasuringRanges => this.measuringRanges;

        public IReadOnlyList<Register> SummaryRegister => this.summaryRegister;
    }
}
