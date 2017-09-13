namespace TRuDI.Models
{
    using System;
    using System.Linq;

    using TRuDI.Models.BasicData;

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

        public static (int Ok, int Warning, int TempError1, int TempError2, int FatalError) GetStatusCount(this IntervalBlock block)
        {
            int ok = 0;
            int warning = 0;
            int tempError1 = 0;
            int tempError2 = 0;
            int fatalError = 0;

            for (int i = 0; i < block.IntervalReadings.Count; i++)
            {
                var statusPtb = block.IntervalReadings[i].StatusPTB.HasValue
                                    ? block.IntervalReadings[i].StatusPTB.Value
                                    : block.IntervalReadings[i].StatusFNN.MapToStatusPtb();

                switch (statusPtb)
                {
                    case StatusPTB.No_Error:
                        ok++;
                        break;

                    case StatusPTB.Warning:
                        warning++;
                        break;

                    case StatusPTB.Temp_Error_signed_invalid:
                        tempError1++;
                        break;

                    case StatusPTB.Temp_Error_is_invalid:
                        tempError2++;
                        break;

                    case StatusPTB.Fatal_Error:
                        fatalError++;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return (ok, warning, tempError1, tempError2, fatalError);
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