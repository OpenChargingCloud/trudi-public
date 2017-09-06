using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

namespace TRuDI.HanAdapter.XmlValidation
{
    public class XmlBuilder
    {
        private XNamespace ar = XNamespace.Get("http://vde.de/AR_2418-6.xsd");
        private XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
        private XNamespace schemaLocation = XNamespace.Get("http://vde.de/AR_2418-6.xsd AR_2418-6.xsd");
        private XNamespace espi = XNamespace.Get("http://naesb.org/espi");
        private XNamespace atom = XNamespace.Get("http://www.w3.org/2005/Atom");

        public ushort ServiceCategoryKind { get; set; }
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

        private bool HasAllProperties()
        {
            return
                !String.IsNullOrEmpty(CustomerId) &&
                !String.IsNullOrEmpty(InvoicingPartyId) &&
                !String.IsNullOrEmpty(SmgwId) &&
                !String.IsNullOrEmpty(Certificate) &&
                !String.IsNullOrEmpty(UsagePointId) &&
                !String.IsNullOrEmpty(TariffName);
        }

        public XDocument GenerateXmlDocument()
        {
            if (!HasAllProperties())
                return new XDocument();

            XElement serviceCategory = new XElement(espi + "ServiceCategory", new XElement(espi + "kind", ServiceCategoryKind));
            XElement customer = new XElement(ar + "Customer", new XElement(ar + "customerId", CustomerId));
            XElement invoicingParty = new XElement(ar + "InvoicingParty", new XElement(ar + "invoicingPartyId", InvoicingPartyId));

            XElement smgw = new XElement(ar + "SMGW");
            smgw.Add(new XElement(ar + "certId", CertId));
            smgw.Add(new XElement(ar + "smgwId", SmgwId.WithNameExtension()));

            XElement usagePoint = new XElement(
                ar + "UsagePoint",
                serviceCategory,
                new XElement(ar + "usagePointId", UsagePointId),
                customer,
                invoicingParty,
                smgw
            );
                        

            // Create root element
            XElement UsagePoints = new XElement(
                ar + "UsagePoints",
                new XAttribute("xmlns", ar),
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                new XAttribute(xsi + "schemaLocation", schemaLocation),
                new XAttribute(XNamespace.Xmlns + "espi", espi),
                new XAttribute(XNamespace.Xmlns + "atom", atom),
                usagePoint
            );

            XDocument trudiXml = new XDocument(UsagePoints);

            //Certificate
            XElement cer = new XElement(ar + "Certificate", new XElement(ar + "certId", CertId), new XElement(ar + "certType", CertType));
            cer.Add(new XElement(ar + "certContent", Certificate));
            usagePoint.Add(cer);

            usagePoint.Add(new XElement(ar + "tariffName", TariffName));

            //event logs
            if (LogList != null && LogList.Count > 0)
            {
                foreach (LogEntry log in LogList)
                {
                    var logEntry = new XElement(ar + "LogEntry",
                        new XElement(ar + "LogEvent",
                            new XElement(ar + "level", (byte)log.LogEvent.Level),
                            new XElement(ar + "text", log.LogEvent.Text),
                            new XElement(ar + "outcome", (byte)log.LogEvent.Outcome),
                            new XElement(ar + "timestamp", log.LogEvent.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssK")
                        )));

                    if (log.RecordNumber != null)
                    {
                        logEntry.AddFirst(new XElement(this.ar + "recordNumber", log.RecordNumber));
                    }

                    usagePoint.Add(logEntry);
                }
            }

            if (MeterReadings == null || MeterReadings.Count == 0)
            {
                return trudiXml; //return early to avoid big if statement
            }

            foreach (MeterReading meterReading in MeterReadings)
            {
                XElement mr = new XElement(ar + "MeterReading",
                    new XElement(ar + "Meter", new XElement(ar + "meterId", meterReading.Meters[0].MeterId)),
                    new XElement(ar + "meterReadingId", meterReading.MeterReadingId),
                    new XElement(ar + "ReadingType",
                        new XElement(espi + "powerOfTenMultiplier", (short)meterReading.ReadingType.PowerOfTenMultiplier),
                        new XElement(espi + "uom", (ushort)meterReading.ReadingType.Uom),
                        new XElement(ar + "scaler", meterReading.ReadingType.Scaler),
                        new XElement(ar + "obisCode", meterReading.ReadingType.ObisCode),
                        new XElement(ar + "qualifiedLogicalName", meterReading.ReadingType.QualifiedLogicalName.WithNameExtension())
                    ));

                foreach (IntervalBlock iBlock in meterReading.IntervalBlocks)
                {
                    XElement intervalBlock = new XElement(ar + "IntervalBlock",
                        new XElement(ar + "interval",
                            new XElement(ar + "duration", iBlock.Interval.Duration),
                            new XElement(ar + "start", iBlock.Interval.Start.ToString("yyyy-MM-ddTHH:mm:ssK"))
                        ));

                    foreach (IntervalReading iReading in iBlock.IntervalReadings)
                    {
                        XElement intervalReading = new XElement(ar + "IntervalReading",
                            new XElement(espi + "value", iReading.Value),
                            new XElement(ar + "timePeriod",
                                new XElement(ar + "duration", iReading.TimePeriod.Duration),
                                new XElement(ar + "start", iReading.TimePeriod.Start.ToString("yyyy-MM-ddTHH:mm:ssK")))
                            );
                        if (iReading.StatusPTB.HasValue)
                        {
                            intervalReading.Add(new XElement(ar + "statusPTB", (int)iReading.StatusPTB));
                        }
                        else
                        {
                            intervalReading.Add(new XElement(ar + "statusFNN", iReading.StatusFNN.Status));
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
