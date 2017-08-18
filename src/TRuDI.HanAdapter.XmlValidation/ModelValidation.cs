﻿namespace TRuDI.HanAdapter.XmlValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

    // In this class are all methods for the post-xml-schema-validation
    public static class ModelValidation
    {

        // The public method for the validation of the model
        public static UsagePointAdapterTRuDI ValidateModel(UsagePointAdapterTRuDI usagePoint)
        {

            List<Exception> exceptions = new List<Exception>();

            if (string.IsNullOrWhiteSpace(usagePoint.UsagePointId))
            {
                exceptions.Add(new InvalidOperationException("The element UsagePointId is invalid."));
            }

            if (string.IsNullOrWhiteSpace(usagePoint.TariffName))
            {
                exceptions.Add(new InvalidOperationException("The element TariffName is invalid."));
            }

            if (usagePoint.InvoicingParty == null)
            {
                exceptions.Add(new InvalidOperationException("UsagePoint does not contain an instance of InvoicingParty."));
            }

            if (usagePoint.Smgw == null)
            {
                exceptions.Add(new InvalidOperationException("UsagePoint does not contain an instance of SMGW."));
            }
            else
            {
                ValidateSMGW(usagePoint.Smgw, exceptions);
            }

            if (usagePoint.ServiceCategory == null)
            {
                exceptions.Add(new InvalidOperationException("UsagePoint does not contain an instance of ServiceCategory."));
            }
            else
            {
                ValidateServiceCategoryKind(usagePoint.ServiceCategory.Kind, exceptions);
            }

            if (usagePoint.Customer == null)
            {
                exceptions.Add(new InvalidOperationException("UsagePoint does not contain an instance of Customer."));
            }

            if (usagePoint.Certificates.Count < 1)
            {
                exceptions.Add(new InvalidOperationException("UsagePoint does not contain an instance of Certificate."));
            }
            else
            {
                foreach (Certificate cert in usagePoint.Certificates)
                {
                   ValidateCertificate(cert, exceptions);
                }
            }

            if (usagePoint.MeterReadings.Count < 1)
            {
                exceptions.Add(new InvalidOperationException("UsagePoint does not contain an instance of MeterReading."));
            }
            else
            {
                foreach (MeterReading meterReading in usagePoint.MeterReadings)
                {
                  ValidateMeterReading(meterReading, exceptions);
                }
            }

            if (usagePoint.LogEntries.Count >= 1)
            {
                foreach (LogEntry logEntry in usagePoint.LogEntries)
                {
                    ValidateLogEntry(logEntry, exceptions);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException("Model error:>", exceptions);
            }

            return usagePoint;
        }

        // Validation of the Certificate instance
        private static void ValidateCertificate(Certificate cert, List<Exception> exceptions)
        {
            if (cert.CertType.HasValue)
            {
                ValidateCertificateCertType(cert.CertType, exceptions);
            }
            else
            {
                exceptions.Add(new InvalidOperationException("The element CertType in Certificate is null."));
            }

            if (cert.CertContent == null)
            {
                exceptions.Add(new InvalidOperationException("The certificate has no content."));
            }
        }

        // Validate if the Certification Type is a valid type
        private static void ValidateCertificateCertType(CertType? type, List<Exception> exceptions)
        {
            switch (type)
            {
                case CertType.SBGW_HAN:
                    break;
                case CertType.Signatur:
                    break;
                case CertType.SubCA:
                    break;
                default:
                    exceptions.Add(new InvalidOperationException($"Invalid CerType {type}."));
                    break;
            }
        }

        // Validation of the SMGW instance
        private static void ValidateSMGW(SMGW smgw, List<Exception> exceptions)
        {
            if (smgw.CertIds.Count < 1)
            {
                exceptions.Add(new InvalidOperationException("The SMGW instance does not contain any certIds."));
            }
        }

        // Validate if the kind of the ServiceCategory is valid
        private static void ValidateServiceCategoryKind(Kind? kind, List<Exception> exceptions)
        {
            switch (kind)
            {
                case Kind.cold:
                    break;
                case Kind.communication:
                    break;
                case Kind.electricity:
                    break;
                case Kind.gas:
                    break;
                case Kind.heat:
                    break;
                case Kind.pressure:
                    break;
                case Kind.time:
                    break;
                case Kind.water:
                    break;
                default:
                    exceptions.Add(new InvalidOperationException($"Invalid ServiceCategory Kind {kind}."));
                    break;
            }
        }

        // Validation of an MeterReading instance
        private static void ValidateMeterReading(MeterReading meterReading, List<Exception> exceptions)
        {
            if (string.IsNullOrWhiteSpace(meterReading.MeterReadingId))
            {
                exceptions.Add(new InvalidOperationException("The element MeterReadingId is null."));
            }

            if (meterReading.ReadingType == null)
            {
                exceptions.Add(new InvalidOperationException("MeterReading does not contain an instance of ReadingType."));
            }
            else
            {
                ValidateReadingType(meterReading.ReadingType, exceptions);
            }

            if (meterReading.Meters.Count < 1)
            {
                exceptions.Add(new InvalidOperationException("MeterReading does not contain an instance of Meter."));
            }

            if (meterReading.IntervalBlocks.Count < 1)
            {
                exceptions.Add(new InvalidOperationException("MeterReading does not contain an instance of IntervalBlock."));
            }
            else
            {
                foreach (IntervalBlock intervalBlock in meterReading.IntervalBlocks)
                {
                 ValidateIntervalBlock(intervalBlock, exceptions);
                }
            }
        }

        // Validation of an LogEntry instance
        private static void ValidateLogEntry(LogEntry entry, List<Exception> exceptions)
        {
            if (entry.LogEvent != null)
            {
                ValidateLogEvent(entry.LogEvent, exceptions);
            }
        }

        // Validation of an LogEvent instance
        private static void ValidateLogEvent(LogEvent logEvent, List<Exception> exceptions)
        {
            if (logEvent.Level.HasValue)
            {
                ValidateLogEventLevel(logEvent.Level, exceptions);
            }

            if (logEvent.Outcome.HasValue)
            {
                ValidateLogEventOutcome(logEvent.Outcome, exceptions);
            }
        }

        // Validate if the level of the LogEvent is valid
        private static void ValidateLogEventLevel(Level? level, List<Exception> exceptions)
        {
            switch (level)
            {
                case Level.INFO:
                    break;
                case Level.WARNING:
                    break;
                case Level.ERROR:
                    break;
                case Level.FATAL:
                    break;
                case Level.EXTENSION:
                    break;
                default:
                    exceptions.Add(new InvalidOperationException($"Invalid LogEvent Level {level}."));
                    break;
            }
        }

        // Validate if the outcome of the LogEvent is valid
        private static void ValidateLogEventOutcome(Outcome? outcome, List<Exception> exceptions)
        {
            switch (outcome)
            {
                case Outcome.SUCCESS:
                    break;
                case Outcome.FAILURE:
                    break;
                case Outcome.EXTENSION:
                    break;
                default:
                    exceptions.Add(new InvalidOperationException($"Invalid LogEvent Outcome {outcome}."));
                    break;
            }
        }

        // Validation of a ReadingType instance
        private static void ValidateReadingType(ReadingType readingType, List<Exception> exceptions)
        {
            if (string.IsNullOrWhiteSpace(readingType.ObisCode))
            {
                exceptions.Add(new InvalidOperationException("The element ObisCode in ReadingType is null."));
            }
            else
            {
                if (!readingType.ObisCode.ValidateHexString())
                {
                    exceptions.Add(new FormatException("ReadingType: ObisCode is an invalid hex string."));
                }
            }

            if (string.IsNullOrEmpty(readingType.QualifiedLogicalName))
            {
                exceptions.Add(new InvalidOperationException("The element QualifiedLogicalName in ReadingType is null."));
            }

            if (readingType.PowerOfTenMultiplier.HasValue)
            {
                ValidateReadingTypePowerOfTenMultiplier(readingType.PowerOfTenMultiplier, exceptions);
            }
            else
            {
                exceptions.Add(new InvalidOperationException("The element PowerOfTenMultiplier in ReadingType is null."));
            }

            if (readingType.Uom.HasValue)
            {
                ValidateReadingTypeUom(readingType.Uom, exceptions);
            }
            else
            {
                exceptions.Add(new InvalidOperationException("The element Uom in ReadingType is null."));
            }
        }

        // Validate if the powerOfTenMultiplier of the ReadingType is valid
        private static void ValidateReadingTypePowerOfTenMultiplier(PowerOfTenMultiplier? powerOfTenMultiplier, List<Exception> exceptions)
        {
            switch (powerOfTenMultiplier)
            {
                case PowerOfTenMultiplier.deca:
                    break;
                case PowerOfTenMultiplier.Giga:
                    break;
                case PowerOfTenMultiplier.hecto:
                    break;
                case PowerOfTenMultiplier.kilo:
                    break;
                case PowerOfTenMultiplier.Mega:
                    break;
                case PowerOfTenMultiplier.micro:
                    break;
                case PowerOfTenMultiplier.mili:
                    break;
                case PowerOfTenMultiplier.None:
                    break;
                default:
                    exceptions.Add(new InvalidOperationException($"Invalid ReadingType powerOfTenMultiplier {powerOfTenMultiplier}."));
                    break;
            }
        }

        // Validate if the uom of the ReadingType is valid
        private static void ValidateReadingTypeUom(Uom? uom, List<Exception> exceptions)
        {
            switch (uom)
            {
                case Uom.Ampere:
                    break;
                case Uom.Ampere_hours:
                    break;
                case Uom.Ampere_squared:
                    break;
                case Uom.Apparent_energy:
                    break;
                case Uom.Apparent_power:
                    break;
                case Uom.Cubic_feet:
                    break;
                case Uom.Cubic_feet_per_hour:
                    break;
                case Uom.Cubic_meter:
                    break;
                case Uom.Cubic_meter_per_hour:
                    break;
                case Uom.Frequency:
                    break;
                case Uom.Joule:
                    break;
                case Uom.Not_Applicable:
                    break;
                case Uom.Power_factor:
                    break;
                case Uom.Reactive_energie:
                    break;
                case Uom.Reactive_power:
                    break;
                case Uom.Real_energy:
                    break;
                case Uom.Real_power:
                    break;
                case Uom.US_Gallons:
                    break;
                case Uom.US_Gallons_per_hour:
                    break;
                case Uom.Volltage:
                    break;
                case Uom.Volts_squared:
                    break;
                default:
                    exceptions.Add(new InvalidOperationException($"Invalid ReadingType Uom {uom}."));
                    break;
            }
        }

        // Validation of an IntervalBlock instance
        private static void ValidateIntervalBlock(IntervalBlock intervalBlock, List<Exception> exceptions)
        {
            if (intervalBlock.Interval == null)
            {
                exceptions.Add(new InvalidOperationException("IntervalBlock does not contain an instance of Interval."));
            }
            else
            {
                ValidateInterval(intervalBlock.Interval, "interval", exceptions);
            }

            if (intervalBlock.IntervalReadings.Count < 1)
            {
                exceptions.Add(new InvalidOperationException("IntervalBlock does not contain an instance of IntervalReading."));
            }
            else
            {
                foreach (IntervalReading intervalReading in intervalBlock.IntervalReadings)
                {
                    ValidateIntervalReading(intervalReading, exceptions);
                }
            }
        }

        // Validation of an Interval instance
        private static void ValidateInterval(Interval interval, string name, List<Exception> exceptions)
        {
            if (!interval.Duration.HasValue)
            {
                exceptions.Add(new InvalidOperationException($"The element Duration in {name} is null. "));
            }

            if (interval.Start == null)
            {
                exceptions.Add(new InvalidOperationException($"The element Start in {name} is null. "));
            }
        }

        // Validation of an IntervalReading instance
        private static void ValidateIntervalReading(IntervalReading intervalReading, List<Exception> exceptions)
        {
            if (intervalReading.TimePeriod == null)
            {
                exceptions.Add(new InvalidOperationException("IntervalReading does not contain an instance of TimePeriod."));
            }
            else
            {
                ValidateInterval(intervalReading.TimePeriod, "timePeriod", exceptions);
            }

            if (!intervalReading.Value.HasValue)
            {
                exceptions.Add(new InvalidOperationException("The element value in intervalReading is null."));
            }

            if (string.IsNullOrWhiteSpace(intervalReading.StatusFNN))
            {
                if (intervalReading.StatusPTB.HasValue)
                {
                    ValidateIntervalReadingStatusPTB(intervalReading.StatusPTB, exceptions);
                }
                else
                {
                    exceptions.Add(new InvalidOperationException("In IntervalReading StatusFNN and StatusPTB have no value."));
                }
            }
            else
            {
                var isValidHex = intervalReading.StatusFNN.ValidateHexString();
                if (isValidHex && intervalReading.StatusPTB.HasValue && !(intervalReading.StatusPTB == StatusPTB.No_Error))
                {
                    exceptions.Add(new InvalidOperationException("In IntervalReading StatusFNN and StatusPTB have both a value. Only one is permitted."));
                }
                else if (!isValidHex && !intervalReading.StatusPTB.HasValue)
                {
                    exceptions.Add(new InvalidOperationException("In IntervalReading StatusFNN is not a valid hex value and StatusPTB is null."));
                }
            }
        }

        // Validate if the StatusPTB of the ReadingType is valid
        private static void ValidateIntervalReadingStatusPTB(StatusPTB? statusPtb, List<Exception> exceptions)
        {
            switch (statusPtb)
            {
                case StatusPTB.Fatal_Error:
                    break;
                case StatusPTB.No_Error:
                    break;
                case StatusPTB.Temp_Error_is_invalid:
                    break;
                case StatusPTB.Temp_Error_signed_invalid:
                    break;
                case StatusPTB.Warning:
                    break;
                default:
                    exceptions.Add(new InvalidOperationException($"Invalid IntervalReading StatusPTB {statusPtb}."));
                    break;
            }
        }
    }
}