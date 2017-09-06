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
                this.ValueCount += block.IntervalReadings.Count;
            }

            this.Start = this.meterReading.IntervalBlocks.First().IntervalReadings.First().TimePeriod.Start;
            this.End = this.meterReading.IntervalBlocks.Last().IntervalReadings.Last().TimePeriod.Start;

            this.Meter = this.meterReading.Meters.FirstOrDefault().MeterId;

            this.HistoricValues = CalculateHistoricConsumption();
        }

        public List<HistoricConsumption> HistoricValues { get; }

        public int GapCount { get; }
        public int ValueCount { get; }

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

        public IntervalReading GetReading(DateTime timestamp)
        {
            foreach (var block in this.meterReading.IntervalBlocks)
            {
                var reading = block.IntervalReadings.FirstOrDefault(ir => ir.TimePeriod.Start == timestamp);
                if (reading != null)
                {
                    return reading;
                }
            }

            return null;
        }

        /// <summary>
        /// Historic consumption is needed for following time periods, (if enough data available):
        /// %daysGoingBack% most recent days, day-by-day
        /// %weeksGoingBack% most recent calendar weeks, week-by-week
        /// %monthsGoingBack% most recent calendar months, month-by-month
        /// the most recent calender year
        /// </summary>
        /// <returns></returns>
        private List<HistoricConsumption> CalculateHistoricConsumption()
        {
            var retList = new List<HistoricConsumption>();

            // Dayly Historic Reads
            int daysGoingBack = 7;
            var lastDayEnd = this.End.Date;

            for (int i = 0; i < daysGoingBack; i++)
            {
                if (lastDayEnd < this.Start)
                    break;

                var startTime = lastDayEnd.AddDays(-1);
                var startReading = this.GetReading(startTime);
                var endReading = this.GetReading(lastDayEnd);

                retList.Add(new HistoricConsumption(startReading, endReading, startTime, lastDayEnd, TimeUnit.Day));
                lastDayEnd = lastDayEnd.AddDays(-1);
            }

            //Weekly
            int weeksGoingBack = 4;
            var lastSundayEnd = this.End.Date;

            while (lastSundayEnd.DayOfWeek != DayOfWeek.Monday)
            {
                lastSundayEnd = lastSundayEnd.AddDays(-1);
            }

            for (int i = 0; i < weeksGoingBack; i++)
            {
                if (lastSundayEnd < this.Start)
                    break;

                var startTime = lastSundayEnd.AddDays(-7);
                var startReading = this.GetReading(startTime);
                var endReading = this.GetReading(lastSundayEnd);

                retList.Add(new HistoricConsumption(startReading, endReading, startTime, lastSundayEnd, TimeUnit.Week));
                lastSundayEnd = lastSundayEnd.AddDays(-7);
            }

            //Monthly
            int monthsGoingBack = 15;
            var lastMonthEnd = new DateTime(this.End.Year, this.End.Month, 1);

            for (int i = 0; i < monthsGoingBack; i++)
            {
                if (lastMonthEnd < this.Start)
                    break;

                var startTime = lastMonthEnd.AddMonths(-1);
                var startReading = this.GetReading(startTime);
                var endReading = this.GetReading(lastMonthEnd);

                retList.Add(new HistoricConsumption(startReading, endReading, startTime, lastMonthEnd, TimeUnit.Month));
                lastMonthEnd = lastMonthEnd.AddMonths(-1);
            }

            //Last calendar year
            var lastYearEnd = new DateTime(this.End.Year, 1, 1);
            if (lastYearEnd < this.Start || lastYearEnd.AddYears(-1) < this.Start)
            {
                var startTime = lastYearEnd.AddYears(-1);
                var startReading = this.GetReading(startTime);
                var endReading = this.GetReading(lastYearEnd);

                retList.Add(new HistoricConsumption(startReading, endReading, startTime, lastYearEnd, TimeUnit.Year));
            }

            return retList;
        }
    }
}
