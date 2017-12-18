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
            StringBuilder sb = new StringBuilder();

            if (String.IsNullOrEmpty(this.CustomerId)) sb.Append("CUstomerId ");
            if (String.IsNullOrEmpty(this.InvoicingPartyId)) sb.Append("InvoicingPartyId ");
            if (String.IsNullOrEmpty(this.SmgwId)) sb.Append("SmgwId ");
            if (String.IsNullOrEmpty(this.Certificate)) sb.Append("Certificate ");
            if (String.IsNullOrEmpty(this.UsagePointId)) sb.Append("UsagePointId ");
            if (String.IsNullOrEmpty(this.TariffName)) sb.Append("TariffName ");

            return sb.ToString();
        }

        private bool HasAllProperties()
        {
            return
                !String.IsNullOrEmpty(this.CustomerId) &&
                !String.IsNullOrEmpty(this.InvoicingPartyId) &&
                !String.IsNullOrEmpty(this.SmgwId) &&
                !String.IsNullOrEmpty(this.Certificate) &&
                !String.IsNullOrEmpty(this.UsagePointId) &&
                !String.IsNullOrEmpty(this.TariffName);
        }

        public XDocument GenerateXmlDocument()
        {
            if (!this.HasAllProperties())
                return new XDocument();

            XElement serviceCategory = new XElement(this.espi + "ServiceCategory", new XElement(this.espi + "kind", (ushort)this.ServiceCategoryKind));
            XElement customer = new XElement(this.ar + "Customer", new XElement(this.ar + "customerId", this.CustomerId));
            XElement invoicingParty = new XElement(this.ar + "InvoicingParty", new XElement(this.ar + "invoicingPartyId", this.InvoicingPartyId));

            XElement smgw = new XElement(this.ar + "SMGW");
            smgw.Add(new XElement(this.ar + "certId", this.CertId));
            smgw.Add(new XElement(this.ar + "smgwId", this.SmgwId.WithNameExtension()));

            XElement usagePoint = new XElement(
                this.ar + "UsagePoint",
                serviceCategory,
                new XElement(this.ar + "usagePointId", this.UsagePointId),
                customer,
                invoicingParty,
                smgw
            );
                        

            // Create root element
            XElement UsagePoints = new XElement(
                this.ar + "UsagePoints",
                new XAttribute("xmlns", this.ar),
                new XAttribute(XNamespace.Xmlns + "xsi", this.xsi),
                new XAttribute(this.xsi + "schemaLocation", this.schemaLocation),
                new XAttribute(XNamespace.Xmlns + "espi", this.espi),
                new XAttribute(XNamespace.Xmlns + "atom", this.atom),
                usagePoint
            );

            XDocument trudiXml = new XDocument(UsagePoints);

            //Certificate
            XElement cer = new XElement(this.ar + "Certificate", new XElement(this.ar + "certId", this.CertId), new XElement(this.ar + "certType", this.CertType));
            cer.Add(new XElement(this.ar + "certContent", this.Certificate));
            usagePoint.Add(cer);

            usagePoint.Add(new XElement(this.ar + "tariffName", this.TariffName));

            //event logs
            if (this.LogList != null && this.LogList.Count > 0)
            {
                foreach (LogEntry log in this.LogList)
                {
                    var logEntry = new XElement(this.ar + "LogEntry",
                        new XElement(this.ar + "LogEvent",
                            new XElement(this.ar + "level", (byte)log.LogEvent.Level),
                            new XElement(this.ar + "text", log.LogEvent.Text),
                            new XElement(this.ar + "outcome", (byte)log.LogEvent.Outcome),
                            new XElement(this.ar + "timestamp", log.LogEvent.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssK")
                        )));

                    if (log.RecordNumber != null)
                    {
                        logEntry.AddFirst(new XElement(this.ar + "recordNumber", log.RecordNumber));
                    }

                    usagePoint.Add(logEntry);
                }
            }

            if (this.MeterReadings == null || this.MeterReadings.Count == 0)
            {
                return trudiXml; //return early to avoid big if statement
            }

            foreach (MeterReading meterReading in this.MeterReadings)
            {

                XElement mr = new XElement(
                    this.ar + "MeterReading",
                    new XElement(this.ar + "Meter", new XElement(this.ar + "meterId", meterReading.Meters[0].MeterId)),
                    new XElement(this.ar + "meterReadingId", meterReading.MeterReadingId));

                if (meterReading.ReadingType.MeasurementPeriod == 0)
                {
                    mr.Add( new XElement(
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

                foreach (IntervalBlock iBlock in meterReading.IntervalBlocks)
                {
                    XElement intervalBlock = new XElement(this.ar + "IntervalBlock",
                        new XElement(this.ar + "interval",
                            new XElement(this.ar + "duration", iBlock.Interval.Duration),
                            new XElement(this.ar + "start", iBlock.Interval.Start.ToString("yyyy-MM-ddTHH:mm:ssK"))
                        ));

                    foreach (IntervalReading iReading in iBlock.IntervalReadings)
                    {
                        XElement intervalReading = new XElement(this.ar + "IntervalReading",
                            new XElement(this.espi + "value", iReading.Value),
                            new XElement(this.ar + "timePeriod",
                                new XElement(this.ar + "duration", iReading.TimePeriod.Duration),
                                new XElement(this.ar + "start", iReading.TimePeriod.Start.ToString("yyyy-MM-ddTHH:mm:ssK")))
                            );
                        if (iReading.StatusPTB.HasValue)
                        {
                            intervalReading.Add(new XElement(this.ar + "statusPTB", (int)iReading.StatusPTB));
                        }
                        else
                        {
                            intervalReading.Add(new XElement(this.ar + "statusFNN", iReading.StatusFNN.Status));
                        }
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
            return id?? (id.EndsWith(".sm") ? id : $"{id}.sm");
        }
    }
}
