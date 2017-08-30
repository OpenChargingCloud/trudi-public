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

        public long? InitialReading
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

        public IReadOnlyList<IAccountingDay> AccountingDays => this.accountingDays;

        public IReadOnlyList<Register> SummaryRegister => this.summaryRegister;
    }
}
