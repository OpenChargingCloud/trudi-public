namespace TRuDI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml.Linq;

    using TRuDI.Models.BasicData;

    public class XmlBuilder
    {
        private XNamespace ar = XNamespace.Get("http://vde.de/AR_2418-6.xsd");
        private XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
        private XNamespace schemaLocation = XNamespace.Get("http://vde.de/AR_2418-6.xsd AR_2418-6.xsd");
        private XNamespace espi = XNamespace.Get("http://naesb.org/espi");
        private XNamespace atom = XNamespace.Get("http://www.w3.org/2005/Atom");

        public Kind ServiceCategoryKind { get; set; }
        public string CustomerId { get; set; }
        public string InvoicingPartyId { get; set; }
        public string SmgwId { get; set; }
        public int CertId { get; set; }
        public int CertType { get; set; }
        public string Certificate { get; set; }
        public string TariffName { get; set; }
        public string UsagePointId { get; set; }

        public List<LogEntry> LogList { get; set; }
        public List<MeterReading> MeterReadings { get; set; }

        public string MissingProperties()
        {
            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(this.CustomerId))
            {
                sb.Append("CustomerId ");
            }

            if (string.IsNullOrEmpty(this.InvoicingPartyId))
            {
                sb.Append("InvoicingPartyId ");
            }

            if (string.IsNullOrEmpty(this.SmgwId))
            {
                sb.Append("SmgwId ");
            }

            if (string.IsNullOrEmpty(this.Certificate))
            {
                sb.Append("Certificate ");
            }

            if (string.IsNullOrEmpty(this.UsagePointId))
            {
                sb.Append("UsagePointId ");
            }

            if (string.IsNullOrEmpty(this.TariffName))
            {
                sb.Append("TariffName ");
            }

            return sb.ToString();
        }

        private bool HasAllProperties()
        {
            return
                !string.IsNullOrEmpty(this.CustomerId) &&
                !string.IsNullOrEmpty(this.InvoicingPartyId) &&
                !string.IsNullOrEmpty(this.SmgwId) &&
                !string.IsNullOrEmpty(this.Certificate) &&
                !string.IsNullOrEmpty(this.UsagePointId) &&
                !string.IsNullOrEmpty(this.TariffName);
        }

        public XDocument GenerateXmlDocument()
        {
            if (!this.HasAllProperties())
            {
                return new XDocument();
            }

            var serviceCategory = new XElement(this.espi + "ServiceCategory", new XElement(this.espi + "kind", (ushort)this.ServiceCategoryKind));
            var customer = new XElement(this.ar + "Customer", new XElement(this.ar + "customerId", this.CustomerId));
            var invoicingParty = new XElement(this.ar + "InvoicingParty", new XElement(this.ar + "invoicingPartyId", this.InvoicingPartyId));

            var smgw = new XElement(this.ar + "SMGW");
            smgw.Add(new XElement(this.ar + "certId", this.CertId));
            smgw.Add(new XElement(this.ar + "smgwId", this.SmgwId.WithNameExtension()));

            var usagePoint = new XElement(
                this.ar + "UsagePoint",
                serviceCategory,
                new XElement(this.ar + "usagePointId", this.UsagePointId),
                customer,
                invoicingParty,
                smgw);

            var trudiXml = new XDocument(new XElement(
                this.ar + "UsagePoints",
                new XAttribute("xmlns", this.ar),
                new XAttribute(XNamespace.Xmlns + "xsi", this.xsi),
                new XAttribute(this.xsi + "schemaLocation", this.schemaLocation),
                new XAttribute(XNamespace.Xmlns + "espi", this.espi),
                new XAttribute(XNamespace.Xmlns + "atom", this.atom),
                usagePoint));

            // Certificate
            var cer = new XElement(this.ar + "Certificate", new XElement(this.ar + "certId", this.CertId), new XElement(this.ar + "certType", this.CertType));
            cer.Add(new XElement(this.ar + "certContent", this.Certificate));
            usagePoint.Add(cer);

            usagePoint.Add(new XElement(this.ar + "tariffName", this.TariffName));

            // event logs
            if (this.LogList != null && this.LogList.Count > 0)
            {
                foreach (LogEntry log in this.LogList)
                {
                    var logEntry = new XElement(this.ar + "LogEntry",
                        new XElement(this.ar + "LogEvent",
                            new XElement(this.ar + "level", (byte)log.LogEvent.Level),
                            new XElement(this.ar + "text", log.LogEvent.Text),
                            new XElement(this.ar + "outcome", (byte)log.LogEvent.Outcome),
                            new XElement(this.ar + "timestamp", log.LogEvent.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssK"))));

                    if (log.RecordNumber != null)
                    {
                        logEntry.AddFirst(new XElement(this.ar + "recordNumber", log.RecordNumber));
                    }

                    usagePoint.Add(logEntry);
                }
            }

            if (this.MeterReadings == null || this.MeterReadings.Count == 0)
            {
                return trudiXml;
            }

            foreach (var meterReading in this.MeterReadings)
            {
                var mr = new XElement(
                    this.ar + "MeterReading",
                    new XElement(this.ar + "Meter", new XElement(this.ar + "meterId", meterReading.Meters[0].MeterId)),
                    new XElement(this.ar + "meterReadingId", meterReading.MeterReadingId));

                if (meterReading.ReadingType.MeasurementPeriod == 0)
                {
                    mr.Add(new XElement(
                        this.ar + "ReadingType",
                        new XElement(this.espi + "powerOfTenMultiplier", (short)meterReading.ReadingType.PowerOfTenMultiplier),
                        new XElement(this.espi + "uom", (ushort)meterReading.ReadingType.Uom),
                        new XElement(this.ar + "scaler", meterReading.ReadingType.Scaler),
                        new XElement(this.ar + "obisCode", meterReading.ReadingType.ObisCode),
                        new XElement(this.ar + "qualifiedLogicalName", meterReading.ReadingType.QualifiedLogicalName.WithNameExtension())));
                }
                else
                {
                    mr.Add(new XElement(
                        this.ar + "ReadingType",
                        new XElement(this.espi + "powerOfTenMultiplier", (short)meterReading.ReadingType.PowerOfTenMultiplier),
                        new XElement(this.espi + "uom", (ushort)meterReading.ReadingType.Uom),
                        new XElement(this.ar + "scaler", meterReading.ReadingType.Scaler),
                        new XElement(this.ar + "obisCode", meterReading.ReadingType.ObisCode),
                        new XElement(this.ar + "qualifiedLogicalName", meterReading.ReadingType.QualifiedLogicalName.WithNameExtension()),
                        new XElement(this.ar + "measurementPeriod", meterReading.ReadingType.MeasurementPeriod)));
                }

                foreach (var iBlock in meterReading.IntervalBlocks)
                {
                    var intervalBlock = new XElement(this.ar + "IntervalBlock",
                        new XElement(this.ar + "interval",
                            new XElement(this.ar + "duration", iBlock.Interval.Duration),
                            new XElement(this.ar + "start", iBlock.Interval.Start.ToString("yyyy-MM-ddTHH:mm:ssK"))));

                    foreach (var iReading in iBlock.IntervalReadings)
                    {
                        var intervalReading = new XElement(this.ar + "IntervalReading",
                            new XElement(this.espi + "value", iReading.Value),
                            new XElement(this.ar + "timePeriod",
                                new XElement(this.ar + "duration", iReading.TimePeriod.Duration),
                                new XElement(this.ar + "start", iReading.TimePeriod.Start.ToString("yyyy-MM-ddTHH:mm:ssK"))));

                        if (iReading.TargetTime.HasValue)
                        {
                            intervalReading.Add(new XElement(this.ar + "targetTime", iReading.TargetTime.Value.ToString("yyyy-MM-ddTHH:mm:ssK")));
                        }

                        intervalReading.Add(
                            iReading.StatusPTB.HasValue
                                ? new XElement(this.ar + "statusPTB", (int)iReading.StatusPTB)
                                : new XElement(this.ar + "statusFNN", iReading.StatusFNN.Status));

                        intervalBlock.Add(intervalReading);
                    }

                    mr.Add(intervalBlock);
                }

                usagePoint.Add(mr);
            }

            return trudiXml;
        }
    }

    public static class XmlBuilderExtensions
    {
        public static string WithNameExtension(this string id)
        {
            return id ?? (id.EndsWith(".sm") ? id : $"{id}.sm");
        }
    }
}
