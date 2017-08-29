namespace TRuDI.TafAdapter.Interface
{
    using System;
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
        public AccountingPeriod Calculate(UsagePointAdapterTRuDI device, UsagePointLieferant supplier)
        {
            var accountingPeriod = new AccountingPeriod();
            var tariffCount = supplier.AnalysisProfile.TariffStages.Count;
            var supplierPeriod = supplier.AnalysisProfile.BillingPeriod;

            MeterReading meterReading = null;

            Interval devicePeriod;


            
            if(device.MeterReadings.Count > 1)
            {

                // TODO Auswahl bei mehreren MeterReadings
            }
            else
            {
                meterReading = device.MeterReadings[0];
            }

            devicePeriod = meterReading.IntervalBlocks[0].Interval;

            accountingPeriod.Begin = supplierPeriod.Start;
            accountingPeriod.End = supplierPeriod.GetEnd();

            var timestamp = devicePeriod.Start;
            while(timestamp <= accountingPeriod.End)
            {
                var readings = meterReading.IntervalBlocks[0].IntervalReadings;

                var reading = readings.Where(ts => ts.TimePeriod.Start == supplierPeriod.Start);

                foreach (SpecialDayProfile profile in supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles)
                {
                    var dayTimeProfiles = supplier.AnalysisProfile.TariffChangeTrigger.
                        TimeTrigger.DayProfiles.Where(p => p.DayId == profile.DayId).LastOrDefault().DayTimeProfiles;

                    var startRange = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[0]);
                    var endRange = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[0]);

                    for (int i = 0; i < dayTimeProfiles.Count; i++)
                    {

                        if(i < dayTimeProfiles.Count - 1)
                        {
                            if (dayTimeProfiles[i].TariffNumber != dayTimeProfiles[i + 1].TariffNumber)
                            {
                                endRange = new DateTime().GetDateTimeFromSpecialDayProfile(profile, dayTimeProfiles[i + 1]);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }



                    if (timestamp < supplierPeriod.Start)
                    {
                        continue;
                    }

                }
               

            }
            return accountingPeriod;
        }


    }
}