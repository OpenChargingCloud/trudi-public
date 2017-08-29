namespace TRuDI.TafAdapter.Interface
{
    using System;
    using System.Collections.Generic;

    public class AccountingDay
    {
        public AccountingDay()
        {
            MeasuringRanges = new List<MeasuringRange>();
            SummaryRegister = new List<Register>();
        }

        public DateTime Date
        {
            get; set;
        }

        public decimal Reading
        {
            get; set;
        }

        public List<MeasuringRange> MeasuringRanges
        {
            get; set;
        }

        public List<Register> SummaryRegister
        {
            get; set;
        }
    }
}
