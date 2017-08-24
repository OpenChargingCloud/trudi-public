namespace TRuDI.HanAdapter.XmlValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

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
                DateTime test = new DateTime();
                
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
    }
}
