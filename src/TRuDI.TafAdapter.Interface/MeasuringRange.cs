namespace TRuDI.TafAdapter.Interface
{
    using System;

    public class MeasuringRange
    {
        public DateTime Start
        {
            get; set;
        }

        public DateTime End
        {
            get; set;
        }

        public string TariffId
        {
            get; set;
        }

        public decimal Amount
        {
            get; set;
        }
    }
}
