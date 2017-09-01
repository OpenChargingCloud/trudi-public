using System;
using System.Collections.Generic;
using System.Text;
using TRuDI.TafAdapter.Interface;

namespace TRuDI.TafAdapter.Taf1
{
    public class AccountingPeriod : IAccountingPeriod
    {
        private List<AccountingSection> accountingDays = new List<AccountingSection>();
        private List<Register> summaryRegister = new List<Register>();
        private List<Reading> initialReadings = new List<Reading>();

        public AccountingPeriod(IList<Register> summaryRegister)
        {
            this.summaryRegister = new List<Register>(summaryRegister);
        }

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

        public IReadOnlyList<Reading> InitialReadings => this.initialReadings;

        public IReadOnlyList<IAccountingSection> AccountingDays => this.accountingDays;;

        public IReadOnlyList<Register> SummaryRegister => this.summaryRegister;
    }
}
