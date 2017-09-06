namespace TRuDI.TafAdapter.Taf2
{
    using System;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;
    using TRuDI.TafAdapter.Interface;

    public class MeasuringRange : IMeasuringRange
    {
        public MeasuringRange()
        {
        }

        public MeasuringRange(DateTime start, DateTime end, ushort tariffId, long amount)
        {
            this.Start = start;
            this.End = end;
            this.TariffId = tariffId;
            this.Amount = amount;
        }

        public MeasuringRange(DateTime start, DateTime end, MeterReading reading, long amount)
        {
            this.Start = start;
            this.End = end;
            this.TariffId = 63;
            this.Amount = amount;
        }

        public DateTime Start
        {
            get; set;
        }

        public DateTime End
        {
            get; set;
        }

        public ushort TariffId
        {
            get; set;
        }

        public long Amount
        {
            get; set;
        }
    }
}
