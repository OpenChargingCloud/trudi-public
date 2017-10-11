namespace TRuDI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TRuDI.HanAdapter.Interface;
    using TRuDI.Models.BasicData;
    using TRuDI.Models.CheckData;

    public static class ContextValidation
    {
        // Validation of additional requirements
        public static void ValidateContext(UsagePointAdapterTRuDI usagePoint, AdapterContext ctx)
        {
            var exceptions = new List<Exception>();

            if (ctx != null)
            {
                ValidateMeterReadingCount(usagePoint, ctx, exceptions);
                ValidateMaximumTimeSpan(ctx, exceptions);

                if (ctx.Contract.TafId == TafId.Taf2)
                {
                    ValidateTaf2RegisterDurations(usagePoint, exceptions);
                }

                if (ctx.Contract.TafId == TafId.Taf7)
                {
                    ValidateTaf7MeterReadingsAreOriginalValueLists(usagePoint, exceptions);
                    ValidateTaf7minRegisterPeriod(usagePoint, exceptions);
                    ValidateTaf7PeriodsInInterval(usagePoint, exceptions);
                }
            }

            CompareCertIds(usagePoint, exceptions);
            ValidateCertificateRawData(usagePoint, exceptions);
            ValidateIntervalBlockConsistence(usagePoint, exceptions);

            if (exceptions.Any())
            {
                throw new AggregateException("Context error:>", exceptions);
            }
        }

        // Validation of additional requirements with additional supplier xml (only Taf-7)
        public static void ValidateContext(UsagePointAdapterTRuDI usagePoint, UsagePointLieferant supplierModel, AdapterContext ctx)
        {

            ValidateContext(usagePoint, ctx);

            var exceptions = new List<Exception>();

            ValidateTaf7ModelSupplierCompatibility(usagePoint, supplierModel, exceptions);
            ValidateTaf7SupplierBillingPeriod(usagePoint, supplierModel, exceptions);
            ValidateSupplierModelTariffStageCount(supplierModel, exceptions);
            ValidateSpecialDayProfilesWithinBillingPeriod(supplierModel, exceptions);
            ValidateSupplierModelCompletelyEnrolledCalendar(usagePoint, supplierModel, exceptions);
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
                        exceptions.Add(new InvalidOperationException($"Das Smart Meter Gateway-Zertifikat mit der ID {sId} konnte nicht gefunden werden."));
                    }
                }
            }
            else
            {
                exceptions.Add(new InvalidOperationException("The number of CertIds in SMGW does not match the number of Certificates."));
            }
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
                    exceptions.Add(new InvalidOperationException($"Zertifikat konnte nicht gelesen werden: ID={certificate.CertId}, Typ={certificate.CertType}", e));
                }
            }
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
            for (int i = 0; i < usagePoint.MeterReadings.Count; i++)
            {
                var reading = usagePoint.MeterReadings[i];
                for (int j = i + 1; j < usagePoint.MeterReadings.Count; j++)
                {
                    if (reading.MeterReadingId == usagePoint.MeterReadings[j].MeterReadingId)
                    {
                        exceptions.Add(new InvalidOperationException("The MeterReadingIds are not unique."));
                    }
                }
            }
        }

        // Check if there is a gap between multiple IntervalBlocks or if the overlap
        private static void ValidateIntervalBlockConsistence(UsagePointAdapterTRuDI usagePoint, List<Exception> exceptions)
        {
            foreach (MeterReading reading in usagePoint.MeterReadings)
            {
                var intervalBlocks = reading.IntervalBlocks.OrderBy(ib => ib.Interval.Start).ToList();

                for (int i = 0; i < intervalBlocks.Count; i++)
                {
                    if (i < intervalBlocks.Count - 1)
                    {
                        if (intervalBlocks[i].Interval.GetEnd() < intervalBlocks[i + 1].Interval.Start)
                        {
                            exceptions.Add(new InvalidOperationException("Lücke zwischen zwei Intervallblöcken."));
                        }
                        else if (intervalBlocks[i].Interval.GetEnd() > intervalBlocks[i + 1].Interval.Start)
                        {
                            exceptions.Add(new InvalidOperationException("Es wurden zwei überlappende Intervallblöcke gefunden."));
                        }
                    }
                }

            }
        }

        // Validation of the maximum TimeSpan
        private static void ValidateMaximumTimeSpan(AdapterContext ctx, List<Exception> exceptions)
        {
            var queryStart = ctx.Start;
            var queryEnd = ctx.End;

            if (queryEnd == null)
            {
                return;
            }
            else
            {
                // 3 years have 94694400 seconds
                if ((queryEnd - queryStart).TotalSeconds > 94694400)
                {
                    exceptions.Add(new InvalidOperationException("The maximum time span of 3 years was exceeded."));
                }
            }
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
                        exceptions.Add(new InvalidOperationException("TAF-2: Die Dauer der einzelnen Ablesungen stimmen nicht überein."));
                    }
                }
            }
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
                    exceptions.Add(new InvalidOperationException($"TAF-7: Ungültige Messperiode: {currentPeriod.TotalMinutes:F2} Minuten"));
                }
                else if ((long)currentPeriod.TotalSeconds % (long)smallestPeriod.TotalSeconds != 0)
                {
                    exceptions.Add(new InvalidOperationException($"TAF-7: Die Messperiode ist kein vielfaches von 15 Minuten: {currentPeriod.TotalMinutes:F2} Minuten"));
                }
            }
        }

        // Taf-7: Validate if all meter readings are original value lists
        private static void ValidateTaf7MeterReadingsAreOriginalValueLists(UsagePointAdapterTRuDI model, List<Exception> exceptions)
        {
            foreach (MeterReading reading in model.MeterReadings)
            {
                if (!reading.IsOriginalValueList())
                {
                    exceptions.Add(new InvalidOperationException("TAF-7: The MeterReading is not an Original Value List."));
                }
            }
        }

        // Taf-7: Validate if all IntervalReadings are within the interval of the IntervalBlock
        private static void ValidateTaf7PeriodsInInterval(UsagePointAdapterTRuDI model, List<Exception> exceptions)
        {
            var originalValueLists = model.MeterReadings.Where(mr => mr.IsOriginalValueList());

            foreach (MeterReading reading in originalValueLists)
            {
                foreach (IntervalBlock ib in reading.IntervalBlocks)
                {
                    //when capture times don't match intended read times we need to override the IntervalBlock duration
                    var intervalBlockEnd = ib.Interval.GetEnd();
                    if (ib.Interval.CaptureTime != ib.Interval.Start)
                    {
                        intervalBlockEnd = intervalBlockEnd.AddSeconds((ib.Interval.CaptureTime - ib.Interval.Start).TotalSeconds);
                    }

                    foreach (IntervalReading ir in ib.IntervalReadings)
                    {
                        if (ir.TimePeriod.Start < ib.Interval.Start || ir.TimePeriod.GetEnd() > intervalBlockEnd)
                        {
                            exceptions.Add(new InvalidOperationException("TAF-7: The TimePeriod of an IntervalReading is outside of the enclosing IntervalBlock"));
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

                var obis = new ObisId(reading.ReadingType.ObisCode);

                if (modelInterval.Start > supplierInterval.Start || modelInterval.GetEnd() < supplierInterval.GetEnd())
                {
                    exceptions.Add(new InvalidOperationException($"TAF-7: {obis}: Zeitbereich der abgelesenen Daten ({modelInterval.Start} bis {modelInterval.GetEnd()}) passt nicht zum Zeitbereich der Tarifdatei des Lieferanten ({supplierInterval.Start} bis {supplierInterval.GetEnd()})."));
                }
            }
        }

        // Validate of all the neccessary ids of the model xml match with the ids in the supplier xml
        private static void ValidateTaf7ModelSupplierCompatibility(UsagePointAdapterTRuDI model, UsagePointLieferant supplier, List<Exception> exceptions)
        {
            if (model.UsagePointId != supplier.UsagePointId)
            {
                exceptions.Add(new InvalidOperationException($"TAF-7: Die ID des Zählpunkts \"{model.UsagePointId}\" stimmt nicht mit der ID des Zählpunkts \"{supplier.UsagePointId}\" aus der Tarifdatei des Lieferanten überein."));
            }

            if (model.InvoicingParty.InvoicingPartyId != supplier.InvoicingParty.InvoicingPartyId)
            {
                exceptions.Add(new InvalidOperationException($"TAF-7: Die Rechnungssteller-ID \"{model.InvoicingParty.InvoicingPartyId}\" stimmt nicht mit der Rechnungssteller-ID \"{supplier.InvoicingParty.InvoicingPartyId}\" aus der Tarifdatei des Lieferanten überein."));
            }

            if (model.ServiceCategory.Kind != supplier.ServiceCategory.Kind)
            {
                exceptions.Add(new InvalidOperationException($"TAF-7: Die Service-Kategory \"{model.ServiceCategory.Kind}\" stimmt nicht mit der Service-Kategory \"{supplier.ServiceCategory.Kind}\" aus der Tarifdatei des Lieferanten überein."));
            }

            if (model.Smgw.SmgwId != supplier.Smgw.SmgwId)
            {
                exceptions.Add(new InvalidOperationException($"TAF-7: Die ID des Smart Meter Gateway \"{model.Smgw.SmgwId}\" stimmt nicht mit der ID \"{supplier.Smgw.SmgwId}\" aus der Tarifdatei des Lieferanten überein."));
            }

            if (model.TariffName != supplier.TariffName)
            {
                exceptions.Add(new InvalidOperationException($"TAF-7: Der Tarifname \"{model.TariffName}\" stimmt nicht mit dem Tariffnamen \"{supplier.TariffName}\" aus der Tarifdatei des Lieferanten überein."));
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

                    exceptions.Add(new InvalidOperationException("TAF-7: Die Tarifschaltzeiten in der Tarifdatei des Lieferanten sind nicht für jede 15-Minuten-Messperiode angegeben."));
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
                        exceptions.Add(new InvalidOperationException($"TAF-7: Ungültige Tarif-Nummer innerhalb eines Tagesprofils: {dtProfile.TariffNumber}"));
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
                foreach (var id in dayProfileIds)
                {
                    if (id == spProfile.DayId)
                    {
                        match = true;
                        break;
                    }
                }
                if (!match)
                {
                    exceptions.Add(new InvalidOperationException("TAF-7: The used DayProfile in SpecialDayProfile is invalid."));
                }
            }
        }

        // Validates the maximum amount of tariff stages
        private static void ValidateSupplierModelTariffStageCount(UsagePointLieferant supplier, List<Exception> exceptions)
        {
            if (supplier.AnalysisProfile.TariffStages.Count > 20)
            {
                var stages = supplier.AnalysisProfile.TariffStages;
                int errorRegisterCount = 0;
                foreach (TariffStage stage in stages)
                {
                    var obisId = new ObisId(stage.ObisCode);
                    if (obisId.E == 63)
                    {
                        errorRegisterCount++;
                    }
                }
                if (stages.Count - errorRegisterCount > 20)
                {
                    exceptions.Add(new InvalidOperationException("Es sind maximal 20 Tarifstuffen zulässig."));
                }
            }
        }

        // Check if the delivered supplier xml has an completely enrolled calendar
        private static void ValidateSupplierModelCompletelyEnrolledCalendar(UsagePointAdapterTRuDI model, UsagePointLieferant supplier, List<Exception> exceptions)
        {
            var period = supplier.AnalysisProfile.BillingPeriod;
            var profiles = supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles;

            var sdpCheckList = new Dictionary<DateTime, int>();
            var timestamp = period.Start;
            var hasCounted = false;
            while (timestamp <= period.GetEnd().AddDays(-1))
            {
                sdpCheckList.Add(timestamp, 0);

                timestamp = timestamp.AddDays(1);
            }

            foreach (var profile in profiles)
            {
                if (sdpCheckList.ContainsKey(profile.SpecialDayDate.GetDate()))
                {
                    sdpCheckList[profile.SpecialDayDate.GetDate()] += 1;
                    hasCounted = true;
                }
            }

            foreach (var item in sdpCheckList)
            {
                if (item.Value == 0 && hasCounted)
                {
                    exceptions.Add(new InvalidOperationException($"Tagesprofil für Tag {item.Key:dd.MM.yyy} nicht vorhanden."));
                }
            }
        }

        // Check if any SpecialDayProfiles are within the billing period
        private static void ValidateSpecialDayProfilesWithinBillingPeriod(UsagePointLieferant supplier, List<Exception> exceptions)
        {
            var begin = supplier.AnalysisProfile.BillingPeriod.Start;
            var end = supplier.AnalysisProfile.BillingPeriod.GetEnd();
            var counter = 0;

            foreach (SpecialDayProfile profile in supplier.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles)
            {
                if (profile.SpecialDayDate.GetDate() >= begin && profile.SpecialDayDate.GetDate() <= end)
                {
                    counter++;
                }
            }

            if (counter == 0)
            {
                exceptions.Add(new InvalidOperationException("Die Tarifdatei des Lieferanten enthält keine Tagesprofile für die Abrechnungsperiode."));
            }
        }
    }
}
