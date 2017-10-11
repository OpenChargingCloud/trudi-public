namespace TRuDI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335;

    using TRuDI.Models.BasicData;

    /// <summary>
    /// The class provides access to to orignial value lists.
    /// </summary>
    public class OriginalValueList
    {
        /// <summary>
        /// The meter reading which belongs to the orignial value list.
        /// </summary>
        public MeterReading MeterReading { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OriginalValueList"/> class.
        /// </summary>
        /// <param name="meterReading">The meter reading.</param>
        public OriginalValueList(MeterReading meterReading)
        {
            this.MeterReading = meterReading;

            this.Obis = new ObisId(this.MeterReading.ReadingType.ObisCode);
            this.DisplayUnit = this.MeterReading.ReadingType.Uom.GetDisplayUnit(this.MeterReading.ReadingType.PowerOfTenMultiplier ?? PowerOfTenMultiplier.None);

            this.MeasurementPeriod = meterReading.GetMeasurementPeriod();
            foreach (var block in this.MeterReading.IntervalBlocks)
            {
                this.GapCount += block.GetGapCount(this.MeasurementPeriod);
                this.ValueCount += block.IntervalReadings.Count;

                var statusCount = block.GetStatusCount();
                this.OkCount += statusCount.Ok;
                this.WarningCount += statusCount.Warning;
                this.TempError1Count += statusCount.TempError1;
                this.TempError2Count += statusCount.TempError2;
                this.FatalErrorCount += statusCount.FatalError;
            }

            this.Start = this.MeterReading.IntervalBlocks.First().IntervalReadings.First().TimePeriod.Start;
            this.End = this.MeterReading.IntervalBlocks.Last().IntervalReadings.Last().TimePeriod.Start;

            this.Meter = this.MeterReading.Meters.FirstOrDefault()?.MeterId;

            this.HistoricValues = this.CalculateHistoricConsumption();
        }

        public List<HistoricConsumption> HistoricValues { get; }

        public int GapCount { get; }
        public int ValueCount { get; }
        public int FatalErrorCount { get; set; }
        public int TempError2Count { get; set; }
        public int TempError1Count { get; set; }
        public int WarningCount { get; set; }
        public int OkCount { get; set; }

        public bool HasErrors => this.FatalErrorCount > 0 || this.WarningCount > 0 || this.TempError1Count > 0
                                 || this.TempError2Count > 0 || this.WarningCount > 0;

        public TimeSpan MeasurementPeriod { get; }

        public ObisId Obis { get; }

        public string DisplayUnit { get; }

        public Uom Uom => this.MeterReading.ReadingType.Uom ?? Uom.Not_Applicable;

        public PowerOfTenMultiplier PowerOfTenMultiplier => this.MeterReading.ReadingType.PowerOfTenMultiplier ?? PowerOfTenMultiplier.None;

        public short Scaler => this.MeterReading.ReadingType.Scaler;

        public string Meter { get; }

        public DateTime Start { get; }
        public DateTime End { get; }

        public IEnumerable<IntervalReading> GetReadings(DateTime start, DateTime end)
        {
            var currentTimestamp = this.Start;

            foreach (var block in this.MeterReading.IntervalBlocks)
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

        public List<DailyOvlErrorStatus> GetErrorsList()
        {
            var statusList = new List<DailyOvlErrorStatus>();
            var currentDay = new DailyOvlErrorStatus { Timestamp = DateTime.MinValue.Date };

            foreach (var reading in this.GetReadings(this.Start, this.End))
            {
                if (currentDay.Timestamp != reading.TimePeriod.Start.Date)
                {
                    currentDay = new DailyOvlErrorStatus { Timestamp = reading.TimePeriod.Start.Date };
                    statusList.Add(currentDay);
                }

                currentDay.ValueCount++;

                if (reading.Value == null)
                {
                    currentDay.GapCount++;
                    continue;
                }
                
                switch (reading.StatusPTB ?? reading.StatusFNN.MapToStatusPtb())
                {
                    case StatusPTB.No_Error:
                        currentDay.OkCount++;
                        break;

                    case StatusPTB.Warning:
                        currentDay.WarningCount++;
                        break;

                    case StatusPTB.Temp_Error_signed_invalid:
                        currentDay.TempError1Count++;
                        break;

                    case StatusPTB.Temp_Error_is_invalid:
                        currentDay.TempError2Count++;
                        break;

                    case StatusPTB.Fatal_Error:
                        currentDay.FatalErrorCount++;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return statusList;
        }

        public IntervalReading GetReading(DateTime timestamp)
        {
            foreach (var block in this.MeterReading.IntervalBlocks)
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
            int monthsGoingBack = 36;
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
            // TODO: last 3 years
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
