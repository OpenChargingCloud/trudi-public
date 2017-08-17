namespace TRuDI.HanAdapter.XmlValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

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
                            exceptions.Add(new InvalidOperationException($"Only one LogEvent element is allowed."));
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
                        usagePoint.MeterReadings.LastOrDefault()
                                  .IntervalBlocks.LastOrDefault()
                                  .IntervalReadings.LastOrDefault().StatusFNN = e.Value;
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
                throw new AggregateException("Parsing error:>", exceptions);
            }

            return usagePoint;
        }

        public static UsagePoint ParseSupplierModel(IEnumerable<XElement> elements)
        {
            throw new NotImplementedException();
        }
    }
}
