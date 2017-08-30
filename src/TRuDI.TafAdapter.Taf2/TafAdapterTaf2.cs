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
            
            foreach(MeterReading meterReading in device.MeterReadings)
            {
                if (meterReading.IsOriginalValueList())
                {
                    var specialDayProfiles = supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles;
                    foreach (SpecialDayProfile profile in specialDayProfiles)
                    {
                        var currentDay = GetDayData(profile, dayProfiles, meterReading, supplier);

                        accountingPeriod.Add(currentDay);
                    }
                }
                else
                {
                    continue;
                }
            }

            //accountingPeriod.InitialReading = accountingPeriod.AccountingDays.First().Reading;
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

            currentDay.Reading = startReading.Value;
            currentDay.Date = new DateTime((int)profile.SpecialDayDate.Year,
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
            var match = false;
            var helpindex = index;

            while (!match)
            {
                end = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[helpindex]);

                if (end == start)
                {
                    (end, helpindex) = FindNextValidTime(end, profile, dayTimeProfiles, meterReading, index);

                    if (helpindex + 1 == dayTimeProfiles.Count)
                    {
                        break;
                    }
                }

                var reading = meterReading.IntervalBlocks.FirstOrDefault(ib => ib.Interval.IsDateInIntervalBlock(end))?
                     .IntervalReadings?.FirstOrDefault(ir => ir.TimePeriod.Start == end);

                if(reading != null)
                {
                    match = true;
                }
                else
                {
                    helpindex--;
                }
            }

            return (end, helpindex);
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
                    result = end;
                    break;
                }

                end = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[helpindex]);

                var reading = meterReading.IntervalBlocks.FirstOrDefault(ib => ib.Interval.IsDateInIntervalBlock(end))?
                    .IntervalReadings?.FirstOrDefault(ir => ir.TimePeriod.Start == end);

                if (reading != null)
                {
                    result = end;
                    match = true;
                }
                else
                {
                    helpindex++;
                }
            }
            return (result, helpindex);
        }

        public DateTime SetLastReading(DateTime time, int index)
        {
            if(index == 95 && time.TimeOfDay == new TimeSpan(23,45,00))
            {
                return time.AddSeconds(900);
            }
            return time;
        }

        public (IntervalReading reading, DateTime end) SetIntervalReading(MeterReading meterReading, DateTime end, int index)
        {
            var date = SetLastReading(end, index);
            return (meterReading.IntervalBlocks.FirstOrDefault(ib => ib.Interval.IsDateInIntervalBlock(date))?
                          .IntervalReadings?.FirstOrDefault(ir => ir.TimePeriod.Start == date), date);
        }
    }
}