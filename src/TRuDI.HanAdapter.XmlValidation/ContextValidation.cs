﻿namespace TRuDI.HanAdapter.XmlValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;
    using TRuDI.HanAdapter.XmlValidation.Models.CheckData;

    public static class ContextValidation
    {

        // TODO Überprüfungen die für TAF 1 und TAF 6 spezifisch sind. 

        // Validation of additional requirements
        public static void ValidateContext(UsagePointAdapterTRuDI usagePoint, AdapterContext ctx)
        {
            var exceptions = new List<Exception>();

            CompareCertIds(usagePoint, exceptions);
            //ValidateQualifiedLogicalName(usagePoint, ctx, exceptions);
            ValidateCertificateRawData(usagePoint, exceptions);
            //ValidateSmgwId(usagePoint, exceptions);
            ValidateMeterReadingCount(usagePoint, ctx, exceptions);
            ValidateIntervalBlockConsistence(usagePoint, exceptions);
            //ValidateBillingPeriod(usagePoint, ctx, exceptions);
            ValidateObisCodes(usagePoint, exceptions);

            if (ctx.Contract.TafId == TafId.Taf2)
            {
                ValidateTaf2RegisterDurations(usagePoint, exceptions);
                ValidateTaf2ObisCodeRegister(usagePoint, exceptions);
            }

            if (ctx.Contract.TafId == TafId.Taf7)
            {
                ValidateTaf7MeterReadingsAreOriginalValueLists(usagePoint, exceptions);
                ValidateTaf7minRegisterPeriod(usagePoint, exceptions);
                ValidateTaf7PeriodsInInterval(usagePoint, exceptions);
            }

            if (exceptions.Any())
            {
                throw new AggregateException("Context error:>", exceptions);
            }
        }

        public static void ValidateContext(UsagePointAdapterTRuDI usagePoint, UsagePointLieferant supplierModel, AdapterContext ctx)
        {
            ValidateContext(usagePoint, ctx);

            var exceptions = new List<Exception>();

            ValidateTaf7ModelSupplierCompatibility(usagePoint, supplierModel, exceptions);
            ValidateTaf7SupplierBillingPeriod(usagePoint, supplierModel, exceptions);
            ValidateSupplierModelTariffStageCount(supplierModel, exceptions);
            ValidateSupplierModelCompletelyEnroledCalendar(usagePoint, supplierModel, exceptions);
            ValidateTaf7SupplierDayProfiles(supplierModel, exceptions);
            ValidateTarifStageOccurence(supplierModel, exceptions);
            ValidateSupplierModelDayProfileOccurence(supplierModel, exceptions);

            if (exceptions.Any())
            {
                throw new AggregateException("Taf-7 Context error:>", exceptions);
            }
        }

        // Comparision of the CertIds
        private static void CompareCertIds(UsagePointAdapterTRuDI usagePoint, List<Exception> exceptions)
        {
            var smgwCertIds = usagePoint.Smgw.CertIds;
            List<byte> certificateCertIds = new List<byte>();

            foreach (Certificate cert in usagePoint.Certificates)
            {
                certificateCertIds.Add((byte)cert.CertId);
            }

            if (smgwCertIds.Count == certificateCertIds.Count)
            {
                bool match = false;
                foreach (byte sId in smgwCertIds)
                {
                    foreach (byte cId in certificateCertIds)
                    {
                        if (sId == cId)
                        {
                            match = true;
                            break;
                        }
                    }

                    if (!match)
                    {
                        exceptions.Add(new InvalidOperationException($"No match in Certificates for the SMGW certId {sId}."));
                    }
                }
            }
            else
            {
                exceptions.Add(new InvalidOperationException("The number of CertIds in SMGW does not match the number of Certificates."));
            }
        }

        // Check whether the QualifiedLogicalName ist correct
        private static void ValidateQualifiedLogicalName(UsagePointAdapterTRuDI usagePoint, AdapterContext ctx, List<Exception> exceptions)
        {
            // TODO den bzw. die qualifiedLogicalName prüfen ob Sie folgendem Schema entsprechen: <ObisCode>.<meterId || tafId?>.sm 
        }

        // Check if the raw data of the certificates are valid certificates
        private static void ValidateCertificateRawData(UsagePointAdapterTRuDI usagePoint, List<Exception> exceptions)
        {
            foreach (Certificate certificate in usagePoint.Certificates)
            {
                try
                {
                    var cert = certificate.GetCert();
                }
                catch (Exception e)
                {
                    exceptions.Add(new InvalidOperationException("Not able to create a Certificate from the raw data.", e));
                    continue;
                }
            }
        }

        // Validate if the Smgwid is built correct
        private static void ValidateSmgwId(UsagePointAdapterTRuDI usagePoint, List<Exception> exceptions)
        {
            // TODO Überprüfung der smgwId. Wurde Sie korrekt gebildet? 
        }

        // Check if the count of MeterReadings are Valid due to the TAF
        private static void ValidateMeterReadingCount(UsagePointAdapterTRuDI usagePoint, AdapterContext ctx, List<Exception> exceptions)
        {
            var meterReadingCount = usagePoint.MeterReadings.Count;

            if (ctx.BillingPeriod != null && ctx.BillingPeriod.End.HasValue)
            {
                if (ctx.Contract.TafId == TafId.Taf1 && meterReadingCount < 2)
                {
                    exceptions.Add(new InvalidOperationException("TAF-1 needs at least 2 instances of MeterReading."));
                }

                if ((ctx.Contract.TafId == TafId.Taf2) && meterReadingCount < 5)
                {
                    exceptions.Add(new InvalidOperationException("TAF-2 needs at least 5 instances of MeterReading."));
                }
            }
        }

        // Check of the meterReadingIds are unique
        private static void ValidateMeterReadingIds(UsagePointAdapterTRuDI usagePoint, List<Exception> exceptions)
        {
            for(int i = 0; i < usagePoint.MeterReadings.Count; i++)
            {
                var reading = usagePoint.MeterReadings[i];
                for (int j = i+1; j < usagePoint.MeterReadings.Count; j++)
                {
                    if(reading.MeterReadingId == usagePoint.MeterReadings[j].MeterReadingId)
                    {
                        exceptions.Add(new InvalidOperationException("The MeterReadingIds are not unique."));
                    }
                }
            }
        }

        private static void ValidateIntervalBlockConsistence(UsagePointAdapterTRuDI usagePoint, List<Exception> exceptions)
        {
            foreach(MeterReading reading in usagePoint.MeterReadings)
            {
                var intervalBlocks = reading.IntervalBlocks.OrderBy(ib => ib.Interval.Start).ToList();

                for(int i = 0; i < intervalBlocks.Count; i++)
                {
                    if(i < intervalBlocks.Count - 1)
                    {
                        if(intervalBlocks[i].Interval.GetEnd() < intervalBlocks[i + 1].Interval.Start)
                        {
                            exceptions.Add(new InvalidOperationException("There is a Gap between two IntervalBlocks."));
                        }
                        else if (intervalBlocks[i].Interval.GetEnd() > intervalBlocks[i + 1].Interval.Start)
                        {
                            exceptions.Add(new InvalidOperationException("There is an overlap between two IntervalBlocks."));
                        }
                    }
                }

            }
        }

        // Validation of the correct billing period
        private static void ValidateBillingPeriod(UsagePointAdapterTRuDI usagePoint, AdapterContext ctx, List<Exception> exceptions)
        {
            var interval = usagePoint.MeterReadings.FirstOrDefault(mr => mr.IsOriginalValueList()).GetMeterReadingInterval();
            var billingPeriod = ctx.BillingPeriod;
            var queryStart = ctx.Start;
            var queryEnd = ctx.End;

            // 3 years have 94694400 seconds
            if ((queryEnd - queryStart).TotalSeconds > 94694400)
            {
                exceptions.Add(new InvalidOperationException("The maximum time span of 3 years was exceeded."));
            }

            // TAF-7: there is no billing period
            if (billingPeriod == null)
            {
                return;
            }

            if (interval.Start <= billingPeriod.Begin && interval.GetEnd() >= billingPeriod.End)
            {
                return;
            }
            else
            {
                exceptions.Add(new InvalidOperationException("The period of the delivered data is invalid."));
            }
        }

        // Validation of the obisCodes
        private static void ValidateObisCodes(UsagePointAdapterTRuDI usagePoint, List<Exception> exceptions)
        {
            // TODO Handelt es sich bei den ObisCodes um gültige Werte? 
        }

        // Taf-2: Validate if the durations in the different MeterReadings match
        private static void ValidateTaf2RegisterDurations(UsagePointAdapterTRuDI usagePoint, List<Exception> exceptions)
        {
            var interval = usagePoint.MeterReadings.FirstOrDefault(mr => !mr.IsOriginalValueList())?.GetMeterReadingInterval();
            if (interval == null)
            {
                return;
            }

            foreach (MeterReading reading in usagePoint.MeterReadings)
            {
                if (!reading.IsOriginalValueList())
                {
                    var intervalTest = reading.GetMeterReadingInterval();
                    
                    if (interval.Duration != intervalTest.Duration)
                    {
                        exceptions.Add(new InvalidOperationException("Taf-2: The Durations do not match."));
                    }
                }
            }
        }

        // Taf-2: Validate of the obisCodes are correct and match the requirements
        private static void ValidateTaf2ObisCodeRegister(UsagePointAdapterTRuDI usagePoint, List<Exception> exceptions)
        {
            // TODO: Taf-2: Die ObisCodes der Register im Hinblick auf Taf-2 prüfen. Ist mindestens eine originäre Messwertliste vorhanden? etc.
        }

        // Taf-7: Validate if the periods between the IntervalReadings match with 15 minutes or a multiple of 15 minutes
        private static void ValidateTaf7minRegisterPeriod(UsagePointAdapterTRuDI usagePoint, List<Exception> exceptions)
        {
            TimeSpan smallestPeriod = new TimeSpan(0, 15, 0);
            TimeSpan greatestPeriod = new TimeSpan(1096, 0, 0, 0);
            var intervalReadings = usagePoint.MeterReadings[0].IntervalBlocks[0].IntervalReadings;

            intervalReadings = intervalReadings.OrderBy(i => i.TimePeriod.Start).ToList();

            for (int index = 0; index < intervalReadings.Count - 1; index++)
            {
                var before = intervalReadings[index].TimePeriod.Start.GetDateWithoutSeconds();
                var next = intervalReadings[index + 1].TimePeriod.Start.GetDateWithoutSeconds();

                var currentPeriod = next - before;

                if (currentPeriod < smallestPeriod || currentPeriod > greatestPeriod)
                {
                    exceptions.Add(new InvalidOperationException("Taf-7: The period is invalid."));
                }
                else if ((long)currentPeriod.TotalSeconds % (long)smallestPeriod.TotalSeconds != 0)
                {
                    exceptions.Add(new InvalidOperationException("Taf-7: The period is not a multiple of 15 minutes."));
                }
            }
        }

        private static void ValidateTaf7MeterReadingsAreOriginalValueLists(UsagePointAdapterTRuDI model, List<Exception> exceptions)
        {
            foreach(MeterReading reading in model.MeterReadings)
            {
                if (!reading.IsOriginalValueList())
                {
                    exceptions.Add(new InvalidOperationException("Taf-7: The MeterReading is not an Original Value List."));
                }
            }
        }

        // Taf-7: Validate if all IntervalReadings are within the interval of the IntervalBlock
        private static void ValidateTaf7PeriodsInInterval(UsagePointAdapterTRuDI model, List<Exception> exceptions)
        {
            var originalValueLists = model.MeterReadings.Where(mr => mr.IsOriginalValueList());
            
            foreach(MeterReading reading in originalValueLists)
            {
                foreach(IntervalBlock ib in reading.IntervalBlocks)
                {
                    foreach(IntervalReading ir in ib.IntervalReadings)
                    {
                        if(ir.TimePeriod.Start < ib.Interval.Start || ir.TimePeriod.GetEnd() > ib.Interval.GetEnd())
                        {
                            exceptions.Add(new InvalidOperationException("Taf-7: The TimePeriod of an IntervalReading is outside of the enclosing IntervalBlock"));
                        }
                    }
                }
            }
        }

        // Taf-7: Validate if the billing period of the model matches the billing period of the supplier data
        private static void ValidateTaf7SupplierBillingPeriod(UsagePointAdapterTRuDI model, UsagePointLieferant supplier, List<Exception> exceptions)
        {
            var originalValueLists = model.MeterReadings.Where(mr => mr.IsOriginalValueList());
            var supplierInterval = supplier.AnalysisProfile.BillingPeriod;

            foreach (MeterReading reading in originalValueLists)
            {
                var modelInterval = reading.GetMeterReadingInterval();

                if (modelInterval.Start > supplierInterval.Start || modelInterval.GetEnd() < supplierInterval.GetEnd())
                {
                    exceptions.Add(new InvalidOperationException("Taf-7: The model does not match with the billing period of the supplier data."));
                }
            }
        }

        private static void ValidateTaf7ModelSupplierCompatibility(UsagePointAdapterTRuDI model,UsagePointLieferant supplier, List<Exception> exceptions)
        {
            if(model.UsagePointId != supplier.UsagePointId)
            {
                exceptions.Add(new InvalidOperationException("Taf-7: The UsagePointId of the model and the supplier do not match."));
            }

            if(model.InvoicingParty.InvoicingPartyId != supplier.InvoicingParty.InvoicingPartyId)
            {
                exceptions.Add(new InvalidOperationException("Taf-7: The InvoicingPartyId of the model and the supplier do not match."));
            }

            if(model.ServiceCategory.Kind != supplier.ServiceCategory.Kind)
            {
                exceptions.Add(new InvalidOperationException("Taf-7: The kind of th service category  of the model and the supplier do not match."));
            }

            if(model.Smgw.SmgwId != supplier.Smgw.SmgwId)
            {
                exceptions.Add(new InvalidOperationException("Taf-7: The Smgw id  of the model and the supplier do not match."));
            }

            if(model.TariffName != supplier.TariffName)
            {
                exceptions.Add(new InvalidOperationException("Taf-7: The tariff name  of the model and the supplier do not match."));
            }
        }
        


        // Taf-7: Validate if the supplier periods have a duration of 15 minutes
        private static void ValidateTaf7SupplierDayProfiles(UsagePointLieferant supplier, List<Exception> exceptions)
        {
            var profiles = supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles;

            foreach (DayProfile profile in profiles)
            {
                var dtProfiles = profile.DayTimeProfiles;
                for (int i = 0; i < dtProfiles.Count; i++)
                {
                    if (i + 1 == dtProfiles.Count)
                    {
                        break;
                    }

                    var current = new TimeSpan((int)dtProfiles[i].StartTime.Hour, (int)dtProfiles[i].StartTime.Minute, 0);

                    var next = new TimeSpan((int)dtProfiles[i + 1].StartTime.Hour, (int)dtProfiles[i + 1].StartTime.Minute, 0);

                    if ((int)(next - current).TotalSeconds == 900)
                    {
                        continue;
                    }

                    {
                        exceptions.Add(new InvalidOperationException("Taf-7: The supplier calender periods are not 15 minutes."));
                    }
                }
            }
        }

        private static void ValidateSpecialDayProfileBillingPeriod(UsagePointLieferant supplier, List<Exception> exceptions)
        {
            var interval = supplier.AnalysisProfile.BillingPeriod;

            foreach(var profile in supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles)
            {
                var specialDayDate = profile.SpecialDayDate.GetDate();
                if (specialDayDate < interval.Start || specialDayDate > interval.GetEnd())
                {
                    exceptions.Add(new InvalidOperationException("Taf-7: A SpecialDayProfile is outside of the supplier billing period."));
                }
            }

        }

        // Check whether all referenced tarif stages which are used in the DayTimeProfiles are valid
        private static void ValidateTarifStageOccurence(UsagePointLieferant supplier, List<Exception> exceptions)
        {
            var tarifStages = new List<ushort>();

            foreach (TariffStage stage in supplier.AnalysisProfile.TariffStages)
            {
                tarifStages.Add(stage.TariffNumber);
            }

            foreach (DayProfile dayProfile in supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles)
            {
                foreach (DayTimeProfile dtProfile in dayProfile.DayTimeProfiles)
                {
                    bool match = false;
                    foreach (int stage in tarifStages)
                    {
                        if (stage == dtProfile.TariffNumber)
                        {
                            match = true;
                            break;
                        }
                    }
                    if (!match)
                    {
                        exceptions.Add(new InvalidOperationException("Taf-7: The used tariffNumber in an DayTimeProfile is invalid."));
                    }
                }
            }
        }

        // Check whether all referenced DayIds in SpecialDayProfiles are valid DayIds
        private static void ValidateSupplierModelDayProfileOccurence(UsagePointLieferant supplier, List<Exception> exceptions)
        {
            var dayProfileIds = new List<ushort?>();

            foreach (DayProfile profile in supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles)
            {
                dayProfileIds.Add(profile.DayId);
            }

            foreach (SpecialDayProfile spProfile in supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles)
            {
                bool match = false;
                foreach (int id in dayProfileIds)
                {
                    if (id == spProfile.DayId)
                    {
                        match = true;
                        break;
                    }
                }
                if (!match)
                {
                    exceptions.Add(new InvalidOperationException("Taf-7: The used DayProfile in SpecialDayProfile is invalid."));
                }
            }
        }

        private static void ValidateSupplierModelTariffStageCount(UsagePointLieferant supplier, List<Exception> exceptions)
        {
            if(supplier.AnalysisProfile.TariffStages.Count > 20)
            {
                var stages = supplier.AnalysisProfile.TariffStages;
                int errorRegisterCount = 0;
                foreach (TariffStage stage in stages)
                {
                    var obisId = new ObisId(stage.ObisCode);
                    if(obisId.E == 63)
                    {
                        errorRegisterCount++;
                    }
                }
                if(stages.Count - errorRegisterCount > 20)
                {
                    exceptions.Add(new InvalidOperationException("The maximum of 20 tarif stages was exceeded."));
                }
            }
        }

        private static void ValidateSupplierModelCompletelyEnroledCalendar(UsagePointAdapterTRuDI model, UsagePointLieferant supplier, List<Exception> exceptions)
        {
            
            var omlCount = 0;
            foreach(MeterReading reading in model.MeterReadings)
            {
                if (reading.IsOriginalValueList())
                {
                    omlCount++;
                }
            }

            var period = supplier.AnalysisProfile.BillingPeriod;
            var profiles = supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles;
            if (profiles.FirstOrDefault().SpecialDayDate.GetDate() < period.Start || profiles.LastOrDefault().SpecialDayDate.GetDate() > period.GetEnd().AddDays(-1))
            {
                exceptions.Add(new InvalidOperationException("A SpecialDayProfile is outside of the billing period."));
            }

            var sdpCheckList = new Dictionary<DateTime, int>();
            var timestamp = period.Start;
            while (timestamp <= period.GetEnd().AddDays(-1))
            {
                sdpCheckList.Add(timestamp, 0);
              
                timestamp = timestamp.AddDays(1);
            }

            foreach (SpecialDayProfile profile in profiles)
            {
                sdpCheckList[profile.SpecialDayDate.GetDate()] += 1;
            }

            foreach (KeyValuePair<DateTime, int> item in sdpCheckList)
            {
                if(item.Value != omlCount)
                {
                    exceptions.Add(new InvalidOperationException("The count of identical SpecialDayProfiles is invalid."));
                }
            }
        }
    }
}
