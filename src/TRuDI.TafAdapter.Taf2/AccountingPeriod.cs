using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TRuDI.TafAdapter.Interface;

namespace TRuDI.TafAdapter.Taf2
{
    public class AccountingPeriod : IAccountingPeriod
    {
        private List<AccountingDay> accountingDays = new List<AccountingDay>();
        private List<Register> summaryRegister = new List<Register>();
        private List<Reading> initialReadings = new List<Reading>();

        public AccountingPeriod(IList<Register> summaryRegister)
        {
            this.summaryRegister = new List<Register>(summaryRegister);
        }

        public DateTime Begin
        {
            get; set;
        }

        public DateTime End
        {
            get; set;
        }

        public void Add(AccountingDay day)
        {
            foreach (Register reg in this.summaryRegister)
            {
                reg.Amount = reg.Amount + day.SummaryRegister.FirstOrDefault(r => r.TariffId == reg.TariffId).Amount;
            }

            this.accountingDays.Add(day);
        }

        public void AddInitialReading(Reading reading)
        {
            if(this.initialReadings.Count < 1)
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

        public void OrderSections()
        {
            if(this.accountingDays.Count > 1)
            {
                this.accountingDays.OrderBy(day => day.Start);
            }
        }

        public IReadOnlyList<Reading> InitialReadings => this.initialReadings;

        public IReadOnlyList<IAccountingSection> AccountingSections => this.accountingDays;

        public IReadOnlyList<Register> SummaryRegister => this.summaryRegister;
    }
}
