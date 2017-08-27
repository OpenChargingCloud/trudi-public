namespace TRuDI.HanAdapter.XmlValidation
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
        // TODO Methoden zur Validierung des geparsten SupplierModels (Lieferant Adapter)

        // Validation of additional requirements
        public static void ValidateContext(UsagePointAdapterTRuDI usagePoint, AdapterContext ctx)
        {
            var exceptions = new List<Exception>();

            CompareCertIds(usagePoint, exceptions);

            ValidateQualifiedLogicalName(usagePoint, ctx, exceptions);

            ValidateCertificateRawData(usagePoint, exceptions);

            ValidateSmgwId(usagePoint, exceptions);

            ValidateMeterReadingCount(usagePoint, ctx, exceptions);

            ValidateBillingPeriod(usagePoint, ctx, exceptions);

            ValidateObisCodes(usagePoint, exceptions);

            if (ctx.Contract.TafId == TafId.Taf2)
            {
                ValidateTaf2RegisterDurations(usagePoint, exceptions);
                ValidateTaf2ObisCodeRegister(usagePoint, exceptions);
            }

            if (ctx.Contract.TafId == TafId.Taf7)
            {
                ValidateTaf7minRegisterPeriod(usagePoint, exceptions);
                ValidateTaf7PeriodsInBillingPeriod(usagePoint, ctx, exceptions);
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

            ValidateTaf7SupplierBillingPeriod(usagePoint, supplierModel, exceptions);

            ValidateTaf7SupplierDayProfiles(supplierModel, exceptions);

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

            if (ctx.Contract.TafId == TafId.Taf1 && meterReadingCount < 2)
            {
                exceptions.Add(new InvalidOperationException("TAF-1 needs at least 2 instances of MeterReading."));
            }

            if ((ctx.Contract.TafId == TafId.Taf2) && meterReadingCount < 5)
            {
                exceptions.Add(new InvalidOperationException("TAF-2 needs at least 5 instances of MeterReading."));
            }
        }
        
        // Check of the meterReadingIds are unique
        private static void ValidateMeterReadingIds(UsagePointAdapterTRuDI usagePoint, AdapterContext ctx, List<Exception> exceptions)
        {
            // TODO Überprüfen ob die MeterReadingIds eindeutig sind.
        }

        // Validation of the correct billing period
        private static void ValidateBillingPeriod(UsagePointAdapterTRuDI usagePoint, AdapterContext ctx, List<Exception> exceptions)
        {
            var interval = usagePoint.MeterReadings[0].IntervalBlocks[0].Interval;
            var billingPeriod = ctx.BillingPeriod;

            // 3 years have 94694400 seconds
            if (interval.Duration > 94694400)
            {
                exceptions.Add(new InvalidOperationException("The maximum billing period of 3 years was exceeded."));
            }

            if (billingPeriod.Begin != interval.Start)
            {
                exceptions.Add(new InvalidOperationException("Invalid billing period (Begin)"));
            }

            if (billingPeriod.End != interval.GetEnd())
            {
                exceptions.Add(new InvalidOperationException("Invalid billing period (End)"));
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
            uint? duration = usagePoint.MeterReadings[0].IntervalBlocks[0].Interval.Duration;
            foreach (MeterReading reading in usagePoint.MeterReadings)
            {
                foreach (IntervalBlock block in reading.IntervalBlocks)
                {
                    if (duration != block.Interval.Duration)
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

        // Taf-7: Validate if all periods are within the valid billing period
        private static void ValidateTaf7PeriodsInBillingPeriod(UsagePointAdapterTRuDI usagePoint, AdapterContext ctx, List<Exception> exceptions)
        {
            // TODO befinden sich alle Perioden im angegebenen Abrechnungszeitraum?
        } 

        // Taf-7: Validate if the billing period of the model matches the billing period of the supplier data
        private static void ValidateTaf7SupplierBillingPeriod(UsagePointAdapterTRuDI model, UsagePointLieferant supplier, List<Exception> exceptions)
        {
            var modelInterval = model.MeterReadings[0].IntervalBlocks[0].Interval;
            var supplierInterval = supplier.AnalysisProfile.BillingPeriod;

            if(modelInterval.Start != supplierInterval.Start)
            {
                exceptions.Add(new InvalidOperationException("Taf-7: The start timestamp of the model does not match the start timestamp of the supplier data."));
            }

            if(modelInterval.Duration != supplierInterval.Duration)
            {
                exceptions.Add(new InvalidOperationException("Taf-7: The billing period duration of the model and the supplier are different."));
            }
        }

        // Taf-7: Validate if the supplier periods have a duration of 15 minutes
        private static void ValidateTaf7SupplierDayProfiles(UsagePointLieferant supplier, List<Exception> exceptions)
        {
            var profiles = supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles;

            foreach(DayProfile profile in profiles)
            {
                var dtProfiles = profile.DayTimeProfiles;
                for(int i = 0; i < dtProfiles.Count; i++)
                {
                    if(i+1 == dtProfiles.Count)
                    {
                        break;
                    }

                    var current = new TimeSpan((int)dtProfiles[i].StartTime.Hour, (int)dtProfiles[i].StartTime.Minute, 0);
                    
                    var next = new TimeSpan((int)dtProfiles[i+1].StartTime.Hour, (int)dtProfiles[i+1].StartTime.Minute, 0);

                    if ((int)(next - current).TotalSeconds == 900)
                    {
                        continue;
                    }
                    else if (next == new TimeSpan(0,0,0) &&  current == new TimeSpan(23,45,00))
                    {
                        continue;    
                    }
                    else if (next == current && (i+2) < dtProfiles.Count)
                    {
                        var preCurrent = new TimeSpan((int)dtProfiles[i-1].StartTime.Hour, (int)dtProfiles[i-1].StartTime.Minute, 0);
                        var postNext =   new TimeSpan((int)dtProfiles[i + 2].StartTime.Hour, (int)dtProfiles[i + 2].StartTime.Minute, 0);
                        if ((int)(postNext - preCurrent).TotalSeconds == 1800)
                        {
                            continue;
                        }
                    }
                    {
                        exceptions.Add(new InvalidOperationException("Taf-7: The supplier calender periods are not 15 minutes."));
                    }
                }
            }
        }
    }
}
