﻿namespace TRuDI.TafAdapter.Taf1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;
    using TRuDI.HanAdapter.XmlValidation.Models.CheckData;
    using TRuDI.TafAdapter.Interface;

    /// <summary>
    /// Default Taf-1 implementation.
    /// </summary>
    public class TafAdapterTaf1 : ITafAdapter
    {
        /// <summary>
        /// Calculates the derived register for Taf1.
        /// </summary>
        /// <param name="device">Date from the SMGW. There should be just original value lists.</param>
        /// <param name="supplier">The calculation data from the supplier.</param>
        /// <returns>An IAccountingPeriod instance. The object contains the calculated data.</returns>
        public IAccountingPeriod Calculate(UsagePointAdapterTRuDI device, UsagePointLieferant supplier)
        {
            var originalValueList = new List<MeterReading>();
            device.MeterReadings.Where(mr => mr.IsOriginalValueList()).ToList().ForEach(mr => originalValueList.Add(mr));

            CheckOriginalValueList(originalValueList, supplier, device.MeterReadings.Count);

            var accountingPeriod = new AccountingPeriod(supplier.GetRegister());
            accountingPeriod.SetDate(supplier.AnalysisProfile.BillingPeriod.Start, supplier.AnalysisProfile.BillingPeriod.GetEnd());

            var dayProfiles = supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles;

            foreach (MeterReading meterReading in originalValueList)
            {
                var currentDate = meterReading.IntervalBlocks.FirstOrDefault().Interval.Start;
                var end = meterReading.IntervalBlocks.LastOrDefault().Interval.GetEnd();
                var validDayProfiles = dayProfiles.GetValidDayProfilesForMeterReading(new ObisId(meterReading.ReadingType.ObisCode),
                    supplier.AnalysisProfile.TariffStages);

                var startReading = meterReading.GetIntervalReadingFromDate(currentDate);
               
                if (validDayProfiles.Count > 1)
                {
                    throw new InvalidOperationException("Invalid DayProfiles Count.");
                }

                var tariffId = supplier.AnalysisProfile.TariffStages.FirstOrDefault(t => t.TariffNumber == supplier.AnalysisProfile.
                               TariffChangeTrigger.TimeTrigger.DayProfiles.FirstOrDefault(pr => pr.DayId == validDayProfiles[0]).DayTimeProfiles.FirstOrDefault().TariffNumber).TariffNumber;

                var specialDayProfiles = supplier.AnalysisProfile.TariffChangeTrigger
                    .TimeTrigger.SpecialDayProfiles.Where(s => s.DayId == validDayProfiles[0]).OrderBy(s => s.SpecialDayDate.GetDate());

                var checkDayIds = CheckDayIdInPeriod(specialDayProfiles.ToList(), (ushort)validDayProfiles[0]);

                if (checkDayIds)
                {
                    while (currentDate < end)
                    {
                        var result = GetSection(supplier, meterReading, startReading, currentDate, end, tariffId);
                        currentDate = result.currentDate;
                        accountingPeriod.Add(result.section);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Taf1: A tariff change is not allowed.");
                }

                accountingPeriod.AddInitialReading(new Reading()
                {
                    Amount = accountingPeriod.AccountingSections.FirstOrDefault(s => s.Reading.ObisCode == meterReading.ReadingType.ObisCode).Reading.Amount,
                    ObisCode = meterReading.ReadingType.ObisCode
                });
            }
            
            return accountingPeriod;
        }

        /// <summary>
        /// The main calculation method for every section in the billing period.
        /// </summary>
        /// <param name="supplier">Contains the calculation data.</param>
        /// <param name="meterReading">The MeterReading instance with the raw data.</param>
        /// <param name="startReading">The intervalReading at the beginning of the section.</param>
        /// <param name="currentDate">The date on which the section starts.</param>
        /// <param name="end">The date on which the billing period ends.</param>
        /// <param name="tariffId">The valid tariffId.</param>
        /// <returns>The calculated AccountingSection</returns>
        public (AccountingMonth section, DateTime currentDate) GetSection(UsagePointLieferant supplier, MeterReading meterReading, IntervalReading startReading, DateTime currentDate, DateTime end, ushort tariffId)
        {
            var section = new AccountingMonth(supplier.GetRegister())
            {
                Reading = new Reading() { Amount = startReading.Value, ObisCode = meterReading.ReadingType.ObisCode }
            };

            var reading = meterReading.GetIntervalReadingFromDate(currentDate.AddMonths(1));
            if (reading != null)
            {
                var range = new MeasuringRange(currentDate, currentDate.AddMonths(1), tariffId, (long)(reading.Value - startReading.Value));
                section.Add(range);
                section.Start = currentDate;
                currentDate = currentDate.AddMonths(1);
                startReading = reading;
            }
            else
            {
                var searchForValue = true;
                var startDate = currentDate;
                while (searchForValue)
                {
                    currentDate = currentDate.AddMonths(1);
                    reading = meterReading.GetIntervalReadingFromDate(currentDate.AddMonths(1));
                    if (reading != null)
                    {
                        var range = new MeasuringRange(startDate, currentDate.AddMonths(1), tariffId, (long)(reading.Value - startReading.Value));
                        section.Add(range);
                        section.Start = currentDate;
                        currentDate = currentDate.AddMonths(1);
                        startReading = reading;
                        searchForValue = false;
                    }
                    else
                    {
                        if (currentDate.AddMonths(1) >= end)
                        {
                            reading = meterReading.GetIntervalReadingFromDate(end);
                            var range = new MeasuringRange(startDate, end, tariffId, (long)(reading.Value - startReading.Value));
                            section.Add(range);
                            section.Start = currentDate;
                            currentDate = end;
                            startReading = reading;
                            searchForValue = false;
                        }
                    }
                }
            }
            return (section, currentDate);
        }

        /// <summary>
        /// Check if the originalValueList is valid.
        /// </summary>
        /// <param name="originalValueList">The list to check.</param>
        /// <param name="supplier">raw data from the supplier.</param>
        public void CheckOriginalValueList(List<MeterReading> originalValueList, UsagePointLieferant supplier, int meterReadingsCount)
        {
            if (originalValueList.Count > 3)
            {
                throw new InvalidOperationException("The maximum of three source meters was exceeded.");
            }

            if(originalValueList.Count != meterReadingsCount)
            {
                throw new InvalidOperationException("Taf7 calculation error: Invalid MeterReading instance. No original value list.");
            }


            if (supplier.AnalysisProfile.TariffStages.Count > originalValueList.Count)
            {
                throw new InvalidOperationException("The calculation of Taf-1 allows just one tariff stage for each source meter.");
            }
        }

        /// <summary>
        /// The method checks if a specialDayProfile references to another dayId. (In Taf1 just one dayId is allowed (No different tariff stages))
        /// </summary>
        /// <param name="specialDayProfiles">The SpecialDayProfile instances which are checked.</param>
        /// <param name="dayId">The valid dayId.</param>
        /// <returns>True of no other dayId was found.</returns>
        public bool CheckDayIdInPeriod(List<SpecialDayProfile> specialDayProfiles, ushort dayId)
        {
            foreach (SpecialDayProfile profile in specialDayProfiles)
            {
              if(profile.DayId != dayId)
                {
                    return false;
                }
            }

            return true;
        } 
    }
}