namespace TRuDI.TafAdapter.Interface
{
    using System;
    using System.Collections.Generic;

    public class AccountingPeriod
    {
        public AccountingPeriod()
        {
            AccountingDays = new List<AccountingDay>();
            SummaryRegister = new List<Register>();
        }

        public DateTime Begin
        {
            get; set;
        }

        public DateTime End
        {
            get; set;
        }

        public decimal InitialReading
        {
            get; set;
        }

        public List<AccountingDay> AccountingDays
        {
            get; set;
        }

        public List<Register> SummaryRegister
        {
            get; set;
        }
    }
}
