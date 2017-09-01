
namespace TRuDI.TafAdapter.Taf2
{
    using System;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;
    using TRuDI.TafAdapter.Interface;

    public class MeasuringRange : IMeasuringRange
    {
        public MeasuringRange()
        {
        }

        public MeasuringRange(DateTime Start, DateTime End, ushort TariffId, long Amount)
        {
            this.Start = Start;
            this.End = End;
            this.TariffId = TariffId;
            this.Amount = Amount;
        }

        public MeasuringRange(DateTime Start, DateTime End, MeterReading reading, long Amount)
        {
            var obisId = new ObisId(reading.ReadingType.ObisCode);
            var TariffNumber = obisId.C == 1 ? 163 : obisId.C == 2 ? 263 : 63;

            this.Start = Start;
            this.End = End;
            this.TariffId = (ushort)TariffNumber;
            this.Amount = Amount;
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
