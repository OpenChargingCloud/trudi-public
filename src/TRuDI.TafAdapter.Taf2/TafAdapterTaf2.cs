namespace TRuDI.TafAdapter.Taf2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TRuDI.Models;
    using TRuDI.Models.BasicData;
    using TRuDI.Models.CheckData;
    using TRuDI.TafAdapter.Interface;

    /// <summary>
    /// Default TAF-2 implementation.
    /// </summary>
    public class TafAdapterTaf2 : ITafAdapter
    {
        /// <summary>
        /// Calculates the derived registers for Taf2.
        /// </summary>
        /// <param name="device">Data from the SMGW. There should be just original value lists.</param>
        /// <param name="supplier">The calculation data from the supplier.</param>
        /// <returns>An IAccountingPeriod instance. The object contains the calculated data.</returns>
        public IAccountingPeriod Calculate(UsagePointAdapterTRuDI device, UsagePointLieferant supplier)
        {
            var accountingPeriod = new AccountingPeriod(supplier.GetRegister());
            accountingPeriod.SetDates(supplier.AnalysisProfile.BillingPeriod.Start, supplier.AnalysisProfile.BillingPeriod.GetEnd());
            var dayProfiles = supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles;

            foreach (MeterReading meterReading in device.MeterReadings)
            {
                if (meterReading.IsOriginalValueList())
                {
                    var obisId = new ObisId(meterReading.ReadingType.ObisCode);
                    var validDayProfiles = dayProfiles.GetValidDayProfilesForMeterReading(obisId, supplier.AnalysisProfile.TariffStages);
                    var specialDayProfiles = GetSpecialDayProfiles(supplier, validDayProfiles, accountingPeriod.Begin, accountingPeriod.End);

                    CheckUsedLists(validDayProfiles, specialDayProfiles);

                    foreach (SpecialDayProfile profile in specialDayProfiles)
                    {
                        var currentDay = GetDayData(profile, dayProfiles, meterReading, supplier);
                        accountingPeriod.Add(currentDay);
                    }

                    accountingPeriod.AddInitialReading(new Reading()
                    {   Amount = accountingPeriod.AccountingSections.FirstOrDefault().Reading.Amount,
                        ObisCode = meterReading.ReadingType.ObisCode
                    });
                }
                else
                {
                    throw new InvalidOperationException("Taf7 calculation error: Invalid MeterReading instance. No original value list.");
                }
            }

            accountingPeriod.OrderSections();
            return accountingPeriod;
        }

        /// <summary>
        /// The main calculation method for every day in the billing period.
        /// </summary>
        /// <param name="profile">The current SpecialDayProfile</param>
        /// <param name="dayProfiles">A List of all DayProfiles</param>
        /// <param name="meterReading">The MeterReading instance with the raw data.</param>
        /// <param name="supplier">Contains the calculation data.</param>
        /// <returns>The calculated AccountingSection</returns>
        public AccountingDay GetDayData(SpecialDayProfile profile, List<DayProfile> dayProfiles, MeterReading meterReading, UsagePointLieferant supplier)
        {
            var currentDay = new AccountingDay(supplier.GetRegister());
            var dayTimeProfiles = dayProfiles.FirstOrDefault(p => p.DayId == profile.DayId).DayTimeProfiles
                .OrderBy(dtp => new DateTime().GetDateTimeFromSpecialDayProfile(profile, dtp)).ToList();
            var start = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[0]);
            var end = start;
            var startReading = meterReading?.IntervalBlocks?.FirstOrDefault(ib => ib.Interval.IsDateInIntervalBlock(start))
                .IntervalReadings?.FirstOrDefault(ir => ir.TimePeriod.Start == start);

            CheckInitSettings(dayTimeProfiles, startReading);

            currentDay.Reading =  new Reading() { Amount = startReading.Value, ObisCode = meterReading.ReadingType.ObisCode };
            currentDay.Start = profile.SpecialDayDate.GetDate();
            
            MeasuringRange range = null;
            for (int i = 0; i < dayTimeProfiles.Count; i++)
            {
                if (i < dayTimeProfiles.Count - 1)
                {
                    if (dayTimeProfiles[i].TariffNumber != dayTimeProfiles[i + 1].TariffNumber)
                    {
                        end = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[i + 1]);
                        var endReading = SetIntervalReading(meterReading, end, i + 1);

                        if (endReading.reading != null)
                        {
                            range = new MeasuringRange(start, endReading.end, (ushort)dayTimeProfiles[i].TariffNumber, (long)(endReading.reading.Value - startReading.Value));
                        }
                        else
                        {
                            var result = FindLastValidTime(start, end, profile, dayTimeProfiles, meterReading, i);
                            endReading = SetIntervalReading(meterReading, result.end, result.index);
                            if (result.end < end)
                            {
                                range = new MeasuringRange(start, endReading.end, (ushort)dayTimeProfiles[i].TariffNumber, (long)(endReading.reading.Value - startReading.Value));
                            }
                            else
                            {
                                range = new MeasuringRange(start, endReading.end, (long)(endReading.reading.Value - startReading.Value));
                            }

                            if(i == result.index)
                            {
                                currentDay.Add(range, new ObisId(meterReading.ReadingType.ObisCode));
                                var j = result.index;
                                var nextDate = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[j+1]);
                                var nextReading = SetIntervalReading(meterReading, nextDate, j+1);
                                while (nextReading.reading == null)
                                {
                                    j++;
                                    if(j < dayTimeProfiles.Count - 1)
                                    {
                                        nextDate = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[j + 1]);
                                        nextReading = SetIntervalReading(meterReading, nextDate, j + 1);
                                    }
                                    else
                                    {
                                        nextDate = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[j]);
                                        nextReading = SetIntervalReading(meterReading, nextDate, j);
                                        if(nextReading.reading == null)
                                        {
                                            nextReading = endReading;
                                            break;
                                        }
                                    }
                                }
                                range = new MeasuringRange(endReading.end, nextReading.end, (long)(nextReading.reading.Value - endReading.reading.Value));
                                endReading = nextReading;
                                result.index = j + 1;
                            }

                            i = result.index;

                        }

                        start = endReading.end;
                        startReading = endReading.reading;
                        currentDay.Add(range, new ObisId(meterReading.ReadingType.ObisCode));
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    end = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[i]);

                    if(start == end)
                    {
                        break;
                    }
                    else
                    {
                        var endReading = SetIntervalReading(meterReading, end, i);
                        if (endReading.reading != null)
                        {
                            range = new MeasuringRange(start, endReading.end, (ushort)dayTimeProfiles[i].TariffNumber, (long)(endReading.reading.Value - startReading.Value));
                        }
                        else
                        {
                            var result = FindLastValidTime(start, end, profile, dayTimeProfiles, meterReading, i);
                            endReading = SetIntervalReading(meterReading, result.end, i);
                            range = new MeasuringRange(start, endReading.end, (long)(endReading.reading.Value - startReading.Value));
                        }
                        currentDay.Add(range, new ObisId(meterReading.ReadingType.ObisCode));
                    }
                }
            }
            return currentDay;
        }

        /// <summary>
        /// If no coresonding value to the tarffif change time is found this method will be called.
        /// </summary>
        /// <param name="start">The beginning of the tariff stage range.</param>
        /// <param name="end">The current endpoint of the tariff stage range.</param>
        /// <param name="profile">The used SpecialDayProfile instance.</param>
        /// <param name="dayTimeProfiles">The used DayTime profile.</param>
        /// <param name="meterReading">The raw data.</param>
        /// <param name="index">For marking the parent loop index.</param>
        /// <returns>The matching DateTime and the new index for the parent loop.</returns>
        public (DateTime end, int index) FindLastValidTime(DateTime start, DateTime end, SpecialDayProfile profile, 
            List<DayTimeProfile> dayTimeProfiles, MeterReading meterReading, int index)
        {
            var result = end;
            var match = false;
            var helpindex = index;

            while (!match)
            {
                result = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[helpindex]);

                if (result == start)
                {
                    (result, helpindex) = FindNextValidTime(end, profile, dayTimeProfiles, meterReading, index);

                    if (helpindex + 1 == dayTimeProfiles.Count)
                    {
                        break;
                    }
                }

                var reading = meterReading.IntervalBlocks.FirstOrDefault(ib => ib.Interval.IsDateInIntervalBlock(result))?
                     .IntervalReadings?.FirstOrDefault(ir => ir.TimePeriod.Start == result);

                if (reading != null)
                {
                    match = true;
                }
                else
                {
                    helpindex--;
                }
            }
            return (result, helpindex);
        }

        /// <summary>
        /// Is called from FindLastValidTime when no matching vaule was found.
        /// </summary>
        /// <param name="end">The current endpoint of the tariff stage range.</param>
        /// <param name="profile">The used SpecialDayProfile instance.</param>
        /// <param name="dayTimeProfiles">The used DayTime profile.</param>
        /// <param name="meterReading">The raw data.</param>
        /// <param name="index">For marking the parent loop index.</param>
        /// <returns>The matching DateTime and the new index for the parent loop.</returns>
        public (DateTime end, int index) FindNextValidTime(DateTime end, SpecialDayProfile profile,
            List<DayTimeProfile> dayTimeProfiles, MeterReading meterReading, int index)
        {
            DateTime result = end;
            var match = false;
            var helpindex = index;
            while (!match)
            {
                if(helpindex >= dayTimeProfiles.Count)
                {
                    helpindex = helpindex - 1;
                    break;
                }

                result = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[helpindex]);
                var reading = meterReading.IntervalBlocks.FirstOrDefault(ib => ib.Interval.IsDateInIntervalBlock(result))?
                    .IntervalReadings?.FirstOrDefault(ir => ir.TimePeriod.Start == result);

                if (reading != null)
                {
                    match = true;
                }
                else
                {
                    helpindex++;
                }
            }
            return (result, helpindex);
        }

        /// <summary>
        /// If a intervalReading to the tariff change timestamp is needed, this method will be called.
        /// </summary>
        /// <param name="meterReading">The raw data.</param>
        /// <param name="end">The date for the meterReading</param>
        /// <param name="index">the index of the parent loop</param>
        /// <returns>An intervalReading or null</returns>
        public (IntervalReading reading, DateTime end) SetIntervalReading(MeterReading meterReading, DateTime end, int index)
        {
            var date = LocalSetLastReading(end, index);
            return (meterReading.IntervalBlocks.FirstOrDefault(ib => ib.Interval.IsDateInIntervalBlock(date))?
                          .IntervalReadings?.FirstOrDefault(ir => ir.TimePeriod.Start == date), date);

            DateTime LocalSetLastReading(DateTime time, int idx)
            {
                if (idx == 95 && time.TimeOfDay == new TimeSpan(23, 45, 00))
                {
                    return time.AddSeconds(900);
                }
                return time;
            }
        }



        public List<SpecialDayProfile> GetSpecialDayProfiles(UsagePointLieferant supplier, List<ushort?> dayProfiles, DateTime begin, DateTime end)
        {
            return supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger
                .SpecialDayProfiles
                .Where(s => dayProfiles.Contains(s.DayId) && s.SpecialDayDate.GetDate() >= begin && s.SpecialDayDate.GetDate() <= end)
                .OrderBy(s => s.SpecialDayDate.GetDate()).ToList();
        }

        public void CheckUsedLists(List<ushort?> validDayProfiles, List<SpecialDayProfile> specialDayProfiles)
        {
            if (validDayProfiles == null || validDayProfiles.Count < 1)
            {
                throw new InvalidOperationException("Taf7 calculation error: No valid DayProfiles found.");
            }
            if(specialDayProfiles == null || specialDayProfiles.Count < 1)
            {
                throw new InvalidOperationException("Taf7 calculation error: No valid SpecialDayProfiles fount.");
            }
        }

        public void CheckInitSettings(List<DayTimeProfile> dayTimeProfiles, IntervalReading reading)
        {
            if (dayTimeProfiles == null || dayTimeProfiles.Count < 1)
            {
                throw new InvalidOperationException("A valid dayProfile contains no DayTimeProfiles.");
            }

            if (reading == null)
            {
                throw new InvalidOperationException("No valid start value found.");
            }
        }
    }
}