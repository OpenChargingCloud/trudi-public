namespace TRuDI.HanAdapter.XmlValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;
    using TRuDI.HanAdapter.XmlValidation.Models.CheckData;

    using AnalysisProfile = TRuDI.HanAdapter.XmlValidation.Models.CheckData.AnalysisProfile;

    public class XmlModelParser
    {
        public static UsagePointAdapterTRuDI ParseHanAdapterModel(IEnumerable<XElement> elements)
        {
            UsagePointAdapterTRuDI usagePoint = null;
            var exceptions = new List<Exception>();
            var logEventAlreadyExists = false;

            foreach (XElement e in elements)
            {
                switch (e.Name.LocalName)
                {
                    case "UsagePoint":
                        usagePoint = new UsagePointAdapterTRuDI();
                        break;
                    case "usagePointId":
                        usagePoint.UsagePointId = e.Value;
                        break;
                    case "tariffName":
                        usagePoint.TariffName = e.Value;
                        break;
                    case "Customer":
                        usagePoint.Customer = new Customer();
                        break;
                    case "customerId":
                        usagePoint.Customer.CustomerId = e.Value;
                        break;
                    case "InvoicingParty":
                        usagePoint.InvoicingParty = new InvoicingParty();
                        break;
                    case "invoicingPartyId":
                        usagePoint.InvoicingParty.InvoicingPartyId = e.Value;
                        break;
                    case "ServiceCategory":
                        usagePoint.ServiceCategory = new ServiceCategory();
                        break;
                    case "kind":
                        usagePoint.ServiceCategory.Kind = (Kind)Convert.ToInt32(e.Value);
                        break;
                    case "SMGW":
                        usagePoint.Smgw = new SMGW();
                        break;
                    case "certId":
                        if (e.Parent.Name.LocalName == "SMGW")
                        {
                            usagePoint.Smgw.CertIds.Add(Convert.ToByte(e.Value));
                        }
                        else if (e.Parent.Name.LocalName == "Certificate")
                        {
                            usagePoint.Certificates.LastOrDefault().CertId = Convert.ToByte(e.Value);
                        }

                        break;
                    case "smgwId":
                        usagePoint.Smgw.SmgwId = e.Value;
                        break;
                    case "Certificate":
                        usagePoint.Certificates.Add(new Certificate());
                        break;
                    case "certType":
                        usagePoint.Certificates.LastOrDefault().CertType = (CertType)Convert.ToByte(e.Value);
                        break;
                    case "parentCertId":
                        usagePoint.Certificates.LastOrDefault().ParentCertId = Convert.ToByte(e.Value);
                        break;
                    case "certContent":
                        if (e.Value.ValidateHexString())
                        {
                            usagePoint.Certificates.LastOrDefault().HexStringToByteArray(e.Value);
                        }
                        else
                        {
                            exceptions.Add(new InvalidOperationException("certContent is an invalid hex string."));
                        }

                        break;
                    case "LogEntry":
                        usagePoint.LogEntries.Add(new LogEntry());
                        logEventAlreadyExists = false;
                        break;
                    case "recordNumber":
                        usagePoint.LogEntries.LastOrDefault().RecordNumber = Convert.ToUInt32(e.Value);
                        break;
                    case "LogEvent":
                        if (logEventAlreadyExists)
                        {
                            exceptions.Add(new InvalidOperationException("The current LogEntry has already a LogEvent instance"));
                        }
                        else
                        {
                            usagePoint.LogEntries.LastOrDefault().LogEvent = new LogEvent();
                            logEventAlreadyExists = true;
                        }

                        break;
                    case "level":
                        usagePoint.LogEntries.LastOrDefault().LogEvent.Level = (Level)Convert.ToByte(e.Value);
                        break;
                    case "text":
                        usagePoint.LogEntries.LastOrDefault().LogEvent.Text = e.Value;
                        break;
                    case "outcome":
                        usagePoint.LogEntries.LastOrDefault().LogEvent.Outcome = (Outcome)Convert.ToByte(e.Value);
                        break;
                    case "timestamp":
                        usagePoint.LogEntries.LastOrDefault().LogEvent.Timestamp = Convert.ToDateTime(e.Value);
                        break;
                    case "MeterReading":
                        usagePoint.MeterReadings.Add(new MeterReading());
                        usagePoint.MeterReadings.LastOrDefault().UsagePoint = usagePoint;
                        break;
                    case "Meter":
                        usagePoint.MeterReadings.LastOrDefault().Meters.Add(new Meter());
                        break;
                    case "meterId":
                        usagePoint.MeterReadings.LastOrDefault().Meters.Last().MeterId = e.Value;
                        break;
                    case "meterReadingId":
                        usagePoint.MeterReadings.LastOrDefault().MeterReadingId = e.Value;
                        break;
                    case "ReadingType":
                        usagePoint.MeterReadings.LastOrDefault().ReadingType = new ReadingType
                        {
                            MeterReading = usagePoint.MeterReadings.LastOrDefault(),
                        };
                        break;
                    case "powerOfTenMultiplier":
                        if (e.Parent.Name.LocalName == "ReadingType")
                        {
                            usagePoint.MeterReadings.LastOrDefault()
                                      .ReadingType.PowerOfTenMultiplier = (PowerOfTenMultiplier)Convert.ToInt16(e.Value);
                        }

                        break;
                    case "uom":
                        usagePoint.MeterReadings.LastOrDefault()
                                  .ReadingType.Uom = (Uom)Convert.ToUInt16(e.Value);
                        break;
                    case "scaler":
                        usagePoint.MeterReadings.LastOrDefault()
                                  .ReadingType.Scaler = Convert.ToSByte(e.Value);
                        break;
                    case "obisCode":
                        usagePoint.MeterReadings.LastOrDefault()
                                  .ReadingType.ObisCode = e.Value;
                        break;
                    case "qualifiedLogicalName":
                        usagePoint.MeterReadings.LastOrDefault()
                                  .ReadingType.QualifiedLogicalName = e.Value;
                        break;
                    case "IntervalBlock":
                        usagePoint.MeterReadings.LastOrDefault()
                                  .IntervalBlocks.Add(new IntervalBlock());
                        usagePoint.MeterReadings.LastOrDefault()
                                  .IntervalBlocks.LastOrDefault()
                                  .MeterReading = usagePoint.MeterReadings.LastOrDefault();
                        break;
                    case "interval":
                        usagePoint.MeterReadings.LastOrDefault()
                                  .IntervalBlocks.LastOrDefault()
                                  .Interval = new Interval();
                        break;
                    case "duration":
                        if (e.Parent.Name.LocalName == "interval")
                        {
                            usagePoint.MeterReadings.LastOrDefault()
                                      .IntervalBlocks.LastOrDefault()
                                      .Interval.Duration = Convert.ToUInt32(e.Value);
                        }
                        else if (e.Parent.Name.LocalName == "timePeriod")
                        {
                            usagePoint.MeterReadings.LastOrDefault()
                                 .IntervalBlocks.LastOrDefault()
                                 .IntervalReadings.LastOrDefault()
                                 .TimePeriod.Duration = Convert.ToUInt32(e.Value);
                        }

                        break;
                    case "start":
                        if (e.Parent.Name.LocalName == "interval")
                        {
                            usagePoint.MeterReadings.LastOrDefault()
                                      .IntervalBlocks.LastOrDefault()
                                      .Interval.Start = Convert.ToDateTime(e.Value);
                        }
                        else if (e.Parent.Name.LocalName == "timePeriod")
                        {
                            usagePoint.MeterReadings.LastOrDefault()
                                 .IntervalBlocks.LastOrDefault()
                                 .IntervalReadings.LastOrDefault()
                                 .TimePeriod.Start = Convert.ToDateTime(e.Value);
                        }

                        break;
                    case "IntervalReading":
                        usagePoint.MeterReadings.LastOrDefault()
                                  .IntervalBlocks.LastOrDefault()
                                  .IntervalReadings.Add(new IntervalReading());
                        usagePoint.MeterReadings.LastOrDefault()
                                  .IntervalBlocks.LastOrDefault()
                                  .IntervalReadings.LastOrDefault()
                                  .IntervalBlock = usagePoint.MeterReadings.LastOrDefault()
                                                             .IntervalBlocks.LastOrDefault();
                        break;
                    case "value":
                        usagePoint.MeterReadings.LastOrDefault()
                                  .IntervalBlocks.LastOrDefault()
                                  .IntervalReadings.LastOrDefault().Value = Convert.ToInt64(e.Value);
                        break;
                    case "timePeriod":
                        usagePoint.MeterReadings.LastOrDefault()
                                  .IntervalBlocks.LastOrDefault()
                                  .IntervalReadings.LastOrDefault().TimePeriod = new Interval();
                        break;
                    case "statusFNN":
                        if (e.Value.ValidateHexString())
                        {
                            usagePoint.MeterReadings.LastOrDefault()
                                  .IntervalBlocks.LastOrDefault()
                                  .IntervalReadings.LastOrDefault().StatusFNN = new StatusFNN(e.Value);
                        }
                        else
                        {
                            exceptions.Add(new InvalidOperationException("statusFNN is an invalid hex string."));
                        }
                        break;
                    case "statusPTB":
                        usagePoint.MeterReadings.LastOrDefault()
                                  .IntervalBlocks.LastOrDefault()
                                  .IntervalReadings.LastOrDefault().StatusPTB = (StatusPTB)Convert.ToByte(e.Value);
                        break;
                    default:
                        exceptions.Add(new InvalidOperationException($"The element {e.Name.LocalName} could not be mapped."));
                        break;
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException("Han Adapter Parsing error:>", exceptions);
            }

            foreach (var meterReading in usagePoint.MeterReadings)
            {
                meterReading.IntervalBlocks.Sort((a, b) => a.Interval.Start.CompareTo(b.Interval.Start));

                foreach (var block in meterReading.IntervalBlocks)
                {
                    block.IntervalReadings.Sort((a, b) => a.TimePeriod.Start.CompareTo(b.TimePeriod.Start));
                }
            }

            return usagePoint;
        }

        public static UsagePointLieferant ParseSupplierModel(IEnumerable<XElement> elements)
        {
            UsagePointLieferant usagePoint = null;
            var exceptions = new List<Exception>();
            var dayIdAlreadyExists = false;

            foreach(XElement e in elements)
            {
                switch (e.Name.LocalName)
                {
                    case "UsagePoint":
                        usagePoint = new UsagePointLieferant();
                        break;
                    case "usagePointId":
                        usagePoint.UsagePointId = e.Value;
                        break;
                    case "tariffName":
                        usagePoint.TariffName = e.Value;
                        break;
                    case "Customer":
                        usagePoint.Customer = new Customer();
                        break;
                    case "customerId":
                        usagePoint.Customer.CustomerId = e.Value;
                        break;
                    case "InvoicingParty":
                        usagePoint.InvoicingParty = new InvoicingParty();
                        break;
                    case "invoicingPartyId":
                        usagePoint.InvoicingParty.InvoicingPartyId = e.Value;
                        break;
                    case "ServiceCategory":
                        usagePoint.ServiceCategory = new ServiceCategory();
                        break;
                    case "kind":
                        usagePoint.ServiceCategory.Kind = (Kind)Convert.ToInt32(e.Value);
                        break;
                    case "SMGW":
                        usagePoint.Smgw = new SMGW();
                        break;
                    case "certId":
                        if (e.Parent.Name.LocalName == "SMGW")
                        {
                            usagePoint.Smgw.CertIds.Add(Convert.ToByte(e.Value));
                        }
                        else if (e.Parent.Name.LocalName == "Certificate")
                        {
                            usagePoint.Certificates.LastOrDefault().CertId = Convert.ToByte(e.Value);
                        }

                        break;
                    case "smgwId":
                        usagePoint.Smgw.SmgwId = e.Value;
                        break;
                    case "Certificate":
                        usagePoint.Certificates.Add(new Certificate());
                        break;
                    case "certType":
                        usagePoint.Certificates.LastOrDefault().CertType = (CertType)Convert.ToByte(e.Value);
                        break;
                    case "parentCertId":
                        usagePoint.Certificates.LastOrDefault().ParentCertId = Convert.ToByte(e.Value);
                        break;
                    case "certContent":
                        if (e.Value.ValidateHexString())
                        {
                            usagePoint.Certificates.LastOrDefault().HexStringToByteArray(e.Value);
                        }
                        else
                        {
                            exceptions.Add(new InvalidOperationException("certContent is an invalid hex string."));
                        }

                        break;
                    case "AnalysisProfile":
                        usagePoint.AnalysisProfile = new AnalysisProfile();
                        break;
                    case "tariffUseCase":
                        usagePoint.AnalysisProfile.TariffUseCase = (TafId)Convert.ToUInt16(e.Value);
                        break;
                    case "tariffId":
                        usagePoint.AnalysisProfile.TariffId = e.Value;
                        break;
                    case "defaultTariffNumber":
                        usagePoint.AnalysisProfile.DefaultTariffNumber = Convert.ToUInt16(e.Value);
                        break;
                    case "billingPeriod":
                        usagePoint.AnalysisProfile.BillingPeriod = new Interval();
                        break;
                    case "duration":
                        usagePoint.AnalysisProfile.BillingPeriod.Duration = Convert.ToUInt32(e.Value);
                        break;
                    case "start":
                        usagePoint.AnalysisProfile.BillingPeriod.Start = Convert.ToDateTime(e.Value);
                        break;
                    case "TariffStage":
                        usagePoint.AnalysisProfile.TariffStages.Add(new TariffStage());
                        break;
                    case "tariffNumber":
                        if (e.Parent.Name.LocalName == "TariffStage")
                        {
                            usagePoint.AnalysisProfile.TariffStages.LastOrDefault().TariffNumber = Convert.ToUInt16(e.Value);
                        }
                        else if (e.Parent.Name.LocalName == "DayTimeProfile")
                        {
                            usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles.LastOrDefault()
                                      .DayTimeProfiles.LastOrDefault().TariffNumber = Convert.ToUInt16(e.Value);
                        }

                        break;
                    case "description":
                        usagePoint.AnalysisProfile.TariffStages.LastOrDefault().Description = e.Value;
                        break;
                    case "obisCode":
                        usagePoint.AnalysisProfile.TariffStages.LastOrDefault().ObisCode = e.Value;
                        break;
                    case "TariffChangeTrigger":
                        usagePoint.AnalysisProfile.TariffChangeTrigger = new TariffChangeTrigger();
                        break;
                    case "TimeTrigger":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger = new TimeTrigger();
                        break;
                    case "DayProfile":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles.Add(new DayProfile());
                        dayIdAlreadyExists = false;
                        break;
                    case "dayId":
                        if (e.Parent.Name.LocalName == "DayProfile")
                        {
                            if (dayIdAlreadyExists)
                            {
                                exceptions.Add(new InvalidOperationException("Only one dayId element in DayProfile is allowed."));
                            }
                            else
                            {
                                usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles.LastOrDefault()
                                .DayId = Convert.ToUInt16(e.Value);
                                dayIdAlreadyExists = true;
                            }
                        }
                        else if (e.Parent.Name.LocalName == "SpecialDayProfile")
                        {
                            var id = Convert.ToUInt16(e.Value);
                            usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles
                                .LastOrDefault().DayId = id;
                            try
                            {
                                usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles
                                    .LastOrDefault().DayProfile = usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger
                                    .DayProfiles.Single(dp => dp.DayId == id);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                        break;
                    case "DayTimeProfile":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles.LastOrDefault()
                            .DayTimeProfiles.Add(new DayTimeProfile());
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles.LastOrDefault()
                            .DayTimeProfiles.LastOrDefault().DayProfile = usagePoint.AnalysisProfile
                            .TariffChangeTrigger.TimeTrigger.DayProfiles.LastOrDefault();
                        break;
                    case "startTime":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles.LastOrDefault()
                            .DayTimeProfiles.LastOrDefault().StartTime = new TimeVarType();
                        break;
                    case "hour":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles.LastOrDefault()
                            .DayTimeProfiles.LastOrDefault().StartTime.Hour = Convert.ToByte(e.Value);
                        break;
                    case "minute":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles.LastOrDefault()
                           .DayTimeProfiles.LastOrDefault().StartTime.Minute = Convert.ToByte(e.Value);
                        break;
                    case "second":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles.LastOrDefault()
                           .DayTimeProfiles.LastOrDefault().StartTime.Second = Convert.ToByte(e.Value);
                        break;
                    case "hundreds":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.DayProfiles.LastOrDefault()
                           .DayTimeProfiles.LastOrDefault().StartTime.Hundreds = Convert.ToByte(e.Value);
                        break;
                    case "SpecialDayProfile":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles.Add(new SpecialDayProfile());
                        break;
                    case "specialDayDate":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles.LastOrDefault()
                            .SpecialDayDate = new DayVarType();
                        break;
                    case "year":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles.LastOrDefault()
                           .SpecialDayDate.Year = Convert.ToUInt16(e.Value);
                        break;
                    case "month":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles.LastOrDefault()
                          .SpecialDayDate.Month = Convert.ToByte(e.Value);
                        break;
                    case "day_of_month":
                        usagePoint.AnalysisProfile.TariffChangeTrigger.TimeTrigger.SpecialDayProfiles.LastOrDefault()
                          .SpecialDayDate.DayOfMonth = Convert.ToByte(e.Value);
                        break;
                    default:
                        exceptions.Add(new InvalidOperationException($"The element {e.Name.LocalName} could not be mapped."));
                        break;
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException("Supplier Parsing error:>", exceptions);
            }

            return usagePoint;
        }
    }
}
