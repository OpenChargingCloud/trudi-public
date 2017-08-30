namespace TRuDI.HanAdapter.XmlValidation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

    public class OriginalValueList
    {
        private readonly MeterReading meterReading;

        public OriginalValueList(MeterReading meterReading)
        {
            this.meterReading = meterReading;

            this.Obis = new ObisId(this.meterReading.ReadingType.ObisCode);
            this.DisplayUnit = this.meterReading.ReadingType.Uom.GetDisplayUnit(this.meterReading.ReadingType.PowerOfTenMultiplier ?? PowerOfTenMultiplier.None);

            this.MeasurementPeriod = meterReading.GetMeasurementPeriod();
            foreach (var block in this.meterReading.IntervalBlocks)
            {
                this.GapCount += block.GetGapCount(this.MeasurementPeriod);
            }

            this.Start = this.meterReading.IntervalBlocks.First().IntervalReadings.First().TimePeriod.Start;
            this.End = this.meterReading.IntervalBlocks.Last().IntervalReadings.Last().TimePeriod.Start;

            this.Meter = this.meterReading.Meters.FirstOrDefault().MeterId;
        }

        public int GapCount { get; }

        public TimeSpan MeasurementPeriod { get; }

        public ObisId Obis { get; }

        public string DisplayUnit { get; }

        public Uom Uom => this.meterReading.ReadingType.Uom ?? Uom.Not_Applicable;

        public PowerOfTenMultiplier PowerOfTenMultiplier => this.meterReading.ReadingType.PowerOfTenMultiplier ?? PowerOfTenMultiplier.None;

        public short Scaler => this.meterReading.ReadingType.Scaler;

        public string Meter { get; }

        public DateTime Start { get; }
        public DateTime End { get; }

        public IEnumerable<IntervalReading> GetReadings(DateTime start, DateTime end)
        {
            var currentTimestamp = this.Start;

            foreach (var block in this.meterReading.IntervalBlocks)
            {
                foreach (var reading in block.IntervalReadings)
                {
                    while (reading.TimePeriod.Start > currentTimestamp)
                    {
                        if (currentTimestamp >= start && currentTimestamp <= end)
                        {
                            // found a gap: create the missing element with only the timestamp.
                            yield return new IntervalReading { TimePeriod = new Interval { Start = currentTimestamp } };
                        }

                        currentTimestamp += this.MeasurementPeriod;
                    }

                    if (reading.TimePeriod.Start > end)
                    {
                        yield break;
                    }

                    if (reading.TimePeriod.Start >= start && reading.TimePeriod.Start <= end)
                    {
                        yield return reading;
                        currentTimestamp += this.MeasurementPeriod;
                    }
                }
            }
        }
    }
}
