namespace TRuDI.TafAdapter.Taf2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;
    using TRuDI.HanAdapter.XmlValidation.Models.CheckData;
    using TRuDI.TafAdapter.Interface;

    /// <summary>
    /// Default TAF-2 implementation.
    /// </summary>
    public class TafAdapterTaf2 : ITafAdapter
    {
        public IAccountingPeriod Calculate(UsagePointAdapterTRuDI device, UsagePointLieferant supplier)
        {
            var accountingPeriod = new AccountingPeriod(supplier.GetRegister());
            var dayProfiles = supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles;

            accountingPeriod.Begin = supplier.AnalysisProfile.BillingPeriod.Start;
            accountingPeriod.End = supplier.AnalysisProfile.BillingPeriod.GetEnd();
            
            foreach(MeterReading meterReading in device.MeterReadings)
            {
                if (meterReading.IsOriginalValueList())
                {
                    var validDayProfiles = dayProfiles.GetValidDayProfilesForMeterReading(new ObisId(meterReading.ReadingType.ObisCode), 
                        supplier.AnalysisProfile.TariffStages);

                    var specialDayProfiles = supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles.Where(s => validDayProfiles.Contains(s.DayId));
                    foreach (SpecialDayProfile profile in specialDayProfiles)
                    {
                        var currentDay = GetDayData(profile, dayProfiles, meterReading, supplier);

                        accountingPeriod.Add(currentDay);
                    }

                    accountingPeriod.AddInitialReading(new Reading()
                    {   Amount = accountingPeriod.AccountingDays.FirstOrDefault().Reading.Amount,
                        ObisCode = meterReading.ReadingType.ObisCode
                    });
                }
                else
                {
                    continue;
                }
            }

            return accountingPeriod;
        }

        public AccountingDay GetDayData(SpecialDayProfile profile, List<DayProfile> dayProfiles, MeterReading meterReading, UsagePointLieferant supplier)
        {
            var currentDay = new AccountingDay(supplier.GetRegister());
            var dayTimeProfiles = dayProfiles.First(p => p.DayId == profile.DayId).DayTimeProfiles;
            DateTime start = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[0]);
            DateTime end = start;

            var startReading = meterReading.IntervalBlocks.FirstOrDefault(ib => ib.Interval.IsDateInIntervalBlock(start))
                .IntervalReadings.FirstOrDefault(ir => ir.TimePeriod.Start == start);

            currentDay.Reading =  new Reading() { Amount = startReading.Value, ObisCode = meterReading.ReadingType.ObisCode };
            currentDay.Start = new DateTime((int)profile.SpecialDayDate.Year,
                                           (int)profile.SpecialDayDate.Month,
                                           (int)profile.SpecialDayDate.DayOfMonth);

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
                            var range = new MeasuringRange(start, endReading.end, (ushort)dayTimeProfiles[i].TariffNumber, (long)(endReading.reading.Value - startReading.Value));
                            start = endReading.end;
                            startReading = endReading.reading;
                            currentDay.Add(range);
                        }
                        else
                        {
                            var result = FindLastValidTime(start, end, profile, dayTimeProfiles, meterReading, i);
                            endReading = SetIntervalReading(meterReading, result.end, result.index);
                            var range = new MeasuringRange(start, endReading.end, meterReading, (long)(endReading.reading.Value - startReading.Value));
                            start = result.end;
                            startReading = endReading.reading;
                            currentDay.Add(range);
                            i = result.index;
                        }
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
                            var range = new MeasuringRange(start, endReading.end, (ushort)dayTimeProfiles[i].TariffNumber, (long)(endReading.reading.Value - startReading.Value));
                            start = endReading.end;
                            startReading = endReading.reading;
                            currentDay.Add(range);
                        }
                        else
                        {
                            var result = FindLastValidTime(start, end, profile, dayTimeProfiles, meterReading, i);
                            endReading = SetIntervalReading(meterReading, result.end, i);
                            var range = new MeasuringRange(start, endReading.end, meterReading, (long)(endReading.reading.Value - startReading.Value));
                            start = endReading.end;
                            startReading = endReading.reading;
                            currentDay.Add(range);
                        }
                    }
                }
            }
            return currentDay;
        }

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
                    (result, helpindex) = FindNextValidTime(result, profile, dayTimeProfiles, meterReading, index);

                    if (helpindex + 1 == dayTimeProfiles.Count)
                    {
                        break;
                    }
                }

                var reading = meterReading.IntervalBlocks.FirstOrDefault(ib => ib.Interval.IsDateInIntervalBlock(result))?
                     .IntervalReadings?.FirstOrDefault(ir => ir.TimePeriod.Start == result);

                if(reading != null)
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
    }
}