namespace TRuDI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TRuDI.Models.BasicData;

    public static class MeterReadingExtensions
    {
        private static readonly int[] MeasuringPeriods = { 900, 60, 120, 180, 240, 300, 600, 1800, 2400, 3600, 5400, 7200, 3 * 3600, 4 * 3600, 6 * 3600, 8 * 3600, 12 * 3600, 24 * 3600, 2592000 };

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

        /// <summary>
        /// Determines the measurement period of the specified interval reading list.
        /// </summary>
        /// <param name="meterReading">The meter reading with interval readings to check.</param>
        /// <param name="checkAll">If set to <c>true</c>, check all interval readings.</param>
        /// <returns>The most frequently found interval or <c>TimeSpan.Zero</c> if no valid interval was found.</returns>
        public static TimeSpan GetMeasurementPeriod(this MeterReading meterReading, bool checkAll = false)
        {
            foreach (var block in meterReading.IntervalBlocks)
            {
                var period = block.IntervalReadings.GetMeasurementPeriod();
                if (period != TimeSpan.Zero)
                {
                    return period;
                }
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// Determines the measurement period of the specified interval reading list.
        /// </summary>
        /// <param name="readings">The interval readings to check.</param>
        /// <param name="checkAll">If set to <c>true</c>, check all interval readings.</param>
        /// <returns>The most frequently found interval or <c>TimeSpan.Zero</c> if no valid interval was found.</returns>
        public static TimeSpan GetMeasurementPeriod(this List<IntervalReading> readings, bool checkAll = false)
        {
            var stats = new Dictionary<int, int>();

            for (int i = 1; i < readings.Count; i++)
            {
                var span = (int)(readings[i].TimePeriod.Start - readings[i - 1].TimePeriod.Start).TotalSeconds;

                var period = GetMatchingMeasurementPeriod(span);
                if (period == 0)
                {
                    continue;
                }

                if (!stats.ContainsKey(period))
                {
                    stats[period] = 1;
                }
                else
                {
                    stats[period]++;
                    if (!checkAll && stats[period] == 10)
                    {
                        // if we don't have to check all: stop after the first period reaches 10
                        return TimeSpan.FromSeconds(period);
                    }
                }
            }

            if (stats.Count == 0)
            {
                return TimeSpan.Zero;
            }

            var list = stats.ToList();
            list.Sort((a, b) => a.Value.CompareTo(b.Value));
            return TimeSpan.FromSeconds(list.Last().Key);
        }

        public static int GetMatchingMeasurementPeriod(int span)
        {
            for (int i = 0; i < MeasuringPeriods.Length; i++)
            {
                int period = MeasuringPeriods[i];
                int window = (period / 100 * 2);

                if (span == period || span > period - window && span < period + window)
                {
                    return period;
                }
            }

            return 0;
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