namespace TRuDI.HanAdapter.XmlValidation.Models
{
    using System;
    using System.Linq;

    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

    public static class MeterReadingExtensions
    {
        public static bool IsOriginalValueList(this MeterReading meterReading)
        {
            var meterId = meterReading.Meters.FirstOrDefault()?.MeterId;
            if (string.IsNullOrWhiteSpace(meterId))
            {
                return false;
            }

            var parts = meterReading.ReadingType.QualifiedLogicalName.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            return parts[1] == meterId;
        }

        public static TimeSpan GetMeasurementPeriod(this MeterReading meterReading)
        {
            var minimalSpan = TimeSpan.FromMinutes(15);
            var intervalSpan = TimeSpan.MaxValue;

            foreach (var block in meterReading.IntervalBlocks)
            {
                for (int i = 1; i < block.IntervalReadings.Count; i++)
                {
                    var span = block.IntervalReadings[i].TimePeriod.Start
                               - block.IntervalReadings[i - 1].TimePeriod.Start;

                    if (span == minimalSpan)
                    {
                        return span;
                    }

                    if (span < intervalSpan)
                    {
                        intervalSpan = span;
                    }
                }

                
            }

            if (intervalSpan == TimeSpan.MaxValue)
            {
                return TimeSpan.Zero;
            }

            return intervalSpan;
        }

        public static int GetGapCount(this IntervalBlock block, TimeSpan measurementPeriod)
        {
            int count = 0;

            for (int i = 1; i < block.IntervalReadings.Count; i++)
            {
                var span = block.IntervalReadings[i].TimePeriod.Start - block.IntervalReadings[i - 1].TimePeriod.Start;
                if (span > measurementPeriod)
                {
                    count++;
                }
            }

            return count;
        }

        public static Interval GetMeterReadingInterval(this MeterReading reading)
        {
            var blocks = reading.IntervalBlocks.OrderBy(ib => ib.Interval.Start).ToList();

            var start = blocks.FirstOrDefault().Interval.Start;
            var end = blocks.LastOrDefault().Interval.GetEnd();
            var duration = (end - start).TotalSeconds;

            return new Interval() { Start = start, Duration = (uint)duration };
        }

        public static IntervalReading GetIntervalReadingFromDate(this MeterReading reading, DateTime date)
        {
           return reading.IntervalBlocks?.FirstOrDefault(ib => ib.Interval.IsDateInIntervalBlock(date))
                .IntervalReadings?.FirstOrDefault(ir => ir.TimePeriod.Start == date);
        }
    }
}