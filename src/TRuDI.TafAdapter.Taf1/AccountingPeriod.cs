using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TRuDI.TafAdapter.Interface;

namespace TRuDI.TafAdapter.Taf1
{
    public class AccountingPeriod : IAccountingPeriod
    {
        private List<AccountingMonth> accountingMonths = new List<AccountingMonth>();
        private List<Register> summaryRegister = new List<Register>();
        private List<Reading> initialReadings = new List<Reading>();

        public AccountingPeriod(IList<Register> summaryRegister)
        {
            this.summaryRegister = new List<Register>(summaryRegister);
        }

        public DateTime Begin { get; private set; }

        public DateTime End { get; private set; }

        public void SetDate(DateTime begin, DateTime end)
        {
            this.Begin = begin;
            this.End = end;
        }

        public void AddInitialReading(Reading reading)
        {
            if (this.initialReadings.Count < 1)
            {
                this.initialReadings.Add(reading);
            }
            else
            {
                if (initialReadings.FirstOrDefault(ir => ir.ObisCode == reading.ObisCode) == null)
                {
                    this.initialReadings.Add(reading);
                }
            }
        }

        public void Add(AccountingMonth month)
        {
            foreach (Register reg in this.summaryRegister)
            {
                reg.Amount = reg.Amount + month.SummaryRegister.FirstOrDefault(r => r.TariffId == reg.TariffId).Amount;
            }

            this.accountingMonths.Add(month);
        }


        public IReadOnlyList<Reading> InitialReadings => this.initialReadings;

        public IReadOnlyList<IAccountingSection> AccountingSections => this.accountingMonths;

        public IReadOnlyList<Register> SummaryRegister => this.summaryRegister;
    }
}
