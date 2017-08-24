namespace TRuDI.HanAdapter.Example.ConfigModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

    public static class XmlFactory
    {
        public static XDocument FabricateHanAdapterContent(HanAdapterExampleConfig hanConfig)
        {
            XNamespace ar = XNamespace.Get("http://vde.de/AR_2418-6.xsd");
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace schemaLocation = XNamespace.Get("http://vde.de/AR_2418-6.xsd AR_2418-6.xsd");
            XNamespace espi = XNamespace.Get("http://naesb.org/espi");
            XNamespace atom = XNamespace.Get("http://www.w3.org/2005/Atom");

            XElement serviceCategory = new XElement(espi + "ServiceCategory", new XElement(espi + "kind", hanConfig.XmlConfig.ServiceCategoryKind));

            XElement customer = new XElement(ar + "Customer", new XElement(ar + "customerId", hanConfig.XmlConfig.CustomerId));

            XElement invoicingParty = new XElement(ar + "InvoicingParty", new XElement(ar + "invoicingPartyId", hanConfig.XmlConfig.InvoicingPartyId));

            XElement smgw = new XElement(ar + "SMGW");

            foreach (CertificateContainer cert in hanConfig.XmlConfig.Certificates)
            {
                smgw.Add(new XElement(ar + "certId", cert.Certificate.CertId));
            }

            smgw.Add(new XElement(ar + "smgwId", hanConfig.XmlConfig.SmgwId));


            XElement usagePoint = new XElement(ar + "UsagePoint",
                                                serviceCategory,
                                                new XElement(ar + "usagePointId", hanConfig.XmlConfig.UsagePointId),
                                                customer,
                                                invoicingParty,
                                                smgw
                                                );

            foreach (CertificateContainer cert in hanConfig.XmlConfig.Certificates)
            {
                var cer = new XElement(ar + "Certificate",
                                            new XElement(ar + "certId", cert.Certificate.CertId),
                                            new XElement(ar + "certType", (byte)cert.Certificate.CertType)
                                           );

                if (cert.Certificate.ParentCertId.HasValue)
                {
                    cer.Add(new XElement(ar + "parentCertId", cert.Certificate.ParentCertId));
                }

                cer.Add(new XElement(ar + "certContent", cert.CertContent));

                usagePoint.Add(cer);
            }

            usagePoint.Add(new XElement(ar + "tariffName", hanConfig.XmlConfig.TariffName));

            if (hanConfig.WithLogData)
            {

                List<LogEntry> logs = LogCreator(hanConfig);

                foreach (LogEntry log in logs)
                {
                    usagePoint.Add(new XElement(ar + "LogEntry",
                                                new XElement(ar + "recordNumber", log.RecordNumber),
                                                new XElement(ar + "LogEvent",
                                                             new XElement(ar + "level", (byte)log.LogEvent.Level),
                                                             new XElement(ar + "text", log.LogEvent.Text),
                                                             new XElement(ar + "outcome", (byte)log.LogEvent.Outcome),
                                                             new XElement(ar + "timestamp", log.LogEvent.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssK")
                                                            )
                                               )
                                  ));
                }
            }

            List<MeterReading> meterReadings = MeterReadingCreator(hanConfig);

            foreach (MeterReading meterReading in meterReadings)
            {
                XElement mr = new XElement(ar + "MeterReading",
                                            new XElement(ar + "Meter", new XElement(ar + "meterId", meterReading.Meters[0].MeterId)),
                                            new XElement(ar + "meterReadingId", meterReading.MeterReadingId),
                                            new XElement(ar + "ReadingType",
                                                         new XElement(espi + "powerOfTenMultiplier", (short)meterReading.ReadingType.PowerOfTenMultiplier),
                                                         new XElement(espi + "uom", (ushort)meterReading.ReadingType.Uom),
                                                         new XElement(ar + "scaler", meterReading.ReadingType.Scaler),
                                                         new XElement(ar + "obisCode", meterReading.ReadingType.ObisCode),
                                                         new XElement(ar + "qualifiedLogicalName", meterReading.ReadingType.QualifiedLogicalName)
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
                            intervalReading.Add(new XElement(ar + "statusPTB", iReading.StatusPTB));
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


            // Create root element
            XElement UsagePoints = new XElement(ar + "UsagePoints",
                                                new XAttribute("xmlns", ar),
                                                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                                                new XAttribute(xsi + "schemaLocation", schemaLocation),
                                                new XAttribute(XNamespace.Xmlns + "espi", espi),
                                                new XAttribute(XNamespace.Xmlns + "atom", atom),
                                                    usagePoint);

            XDocument trudiXml = new XDocument(UsagePoints);

            return trudiXml;
        }
        // TODO FabricateLieferantXml fertig implementieren
        private static XDocument FabricateLieferantXml(HanAdapterExampleConfig hanConfig)
        {
            XNamespace ar = XNamespace.Get("http://vde.de/AR_2418-6.xsd");
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace schemaLocation = XNamespace.Get("http://vde.de/AR_2418-6.xsd AR_2418-6.xsd");
            XNamespace espi = XNamespace.Get("http://naesb.org/espi");
            XNamespace atom = XNamespace.Get("http://www.w3.org/2005/Atom");

            XElement serviceCategory = new XElement(espi + "ServiceCategory", new XElement(espi + "kind", hanConfig.XmlConfig.ServiceCategoryKind));


            XElement customer = new XElement(ar + "Customer", new XElement(ar + "customerId", hanConfig.XmlConfig.CustomerId));

            XElement invoicingParty = new XElement(ar + "InvoicingParty", new XElement(ar + "invoicingPartyId", hanConfig.XmlConfig.InvoicingPartyId));

            XElement smgw = new XElement(ar + "SMGW");

            foreach (CertificateContainer cert in hanConfig.XmlConfig.Certificates)
            {
                smgw.Add(new XElement(ar + "certId", cert.Certificate.CertId));
            }

            smgw.Add(new XElement(ar + "smgwId", hanConfig.XmlConfig.SmgwId));






            XElement usagePoint = new XElement(ar + "UsagePoint",
                                               serviceCategory,
                                               new XElement(ar + "usagePointId", hanConfig.XmlConfig.UsagePointId),
                                               customer,
                                               invoicingParty,
                                               smgw
                                               );





            // Create root element
            XElement UsagePoints = new XElement(ar + "UsagePoints",
                                                new XAttribute("xmlns", ar),
                                                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                                                new XAttribute(xsi + "schemaLocation", schemaLocation),
                                                new XAttribute(XNamespace.Xmlns + "espi", espi),
                                                new XAttribute(XNamespace.Xmlns + "atom", atom),
                                                    usagePoint);

            XDocument lieferantXml = new XDocument(UsagePoints);

            return lieferantXml;

        }

        private static List<LogEntry> LogCreator(HanAdapterExampleConfig hanConfig)
        {
            var logs = new List<LogEntry>();
            TimeSpan minPeriod = new TimeSpan(0, 15, 0);

            if (hanConfig.XmlConfig.RandomLogCount)
            {
                var max = (hanConfig.BillingPeriod.End - hanConfig.BillingPeriod.Begin)?.TotalSeconds / minPeriod.TotalSeconds * 3;
                hanConfig.XmlConfig.LogCount = RandomNumber(1, (int)max);
            }

            var lastLogTime = hanConfig.BillingPeriod.Begin;
            if (!hanConfig.XmlConfig.LogCount.HasValue)
            {
                hanConfig.XmlConfig.LogCount = 3;
            }
            var period = (hanConfig.BillingPeriod.End - lastLogTime) / hanConfig.XmlConfig.LogCount;

            for (int i = 0; i < hanConfig.XmlConfig.LogCount; i++)
            {
                logs.Add(new LogEntry()
                {
                    RecordNumber = (uint)i + 1,
                    LogEvent = new LogEvent()
                    {
                        Level = (Level)RandomNumber(1, 2),
                        Outcome = (Outcome)RandomNumber(0, 2),
                        Text = hanConfig.XmlConfig.PossibleLogMessages[RandomNumber(0, hanConfig.XmlConfig.PossibleLogMessages.Count - 1)],
                        Timestamp = lastLogTime.AddMinutes((double)period?.TotalMinutes)
                    }
                });
                lastLogTime = logs.LastOrDefault().LogEvent.Timestamp;
            }

            return logs;

        }

        private static List<MeterReading> MeterReadingCreator(HanAdapterExampleConfig hanConfig)
        {
            var meterReadings = new List<MeterReading>();
            var meterReadingConfigs = hanConfig.XmlConfig.MeterReadingConfigs;
            var tariffStageCounter = hanConfig.XmlConfig.TariffStageCount;
            int[] taf1Reg = null;
            var usedValue = hanConfig.XmlConfig.ValueSummary;
            var rest = usedValue;

            foreach (MeterReadingConfig meterReadingConfig in meterReadingConfigs)
            {
                var mr = new MeterReading();

                mr.Meters.Add(new Meter() { MeterId = meterReadingConfig.MeterId });

                mr.MeterReadingId = meterReadingConfig.MeterReadingId;

                mr.ReadingType = new ReadingType()
                {
                    MeterReading = mr,
                    PowerOfTenMultiplier = (PowerOfTenMultiplier)meterReadingConfig.PowerOfTenMultiplier,
                    Uom = (Uom)meterReadingConfig.Uom,
                    Scaler = (short)meterReadingConfig.Scaler,
                    ObisCode = meterReadingConfig.ObisCode
                };

                if (meterReadingConfig.IsOML)
                {
                    mr.ReadingType.QualifiedLogicalName = BuildQualifiedLogicalName(meterReadingConfig.MeterId, mr.ReadingType.ObisCode);

                    foreach (IntervalBlockConfig intervalBlockConfig in meterReadingConfig.IntervalBlocks)
                    {

                        mr.IntervalBlocks.Add(CreateIntervalBlockOML(meterReadingConfig, intervalBlockConfig, hanConfig, taf1Reg));
                    }
                }
                else
                {
                    var tarifStage = hanConfig.XmlConfig.TariffStageCount - tariffStageCounter;
                    if (tariffStageCounter == -1)
                    {
                        tarifStage = -1;
                    }
                    mr.ReadingType.QualifiedLogicalName = BuildQualifiedLogicalName(hanConfig.XmlConfig.TariffId, mr.ReadingType.ObisCode);

                    if (meterReadingConfig == meterReadingConfigs.LastOrDefault())
                    {
                        usedValue = usedValue + rest;
                    }

                    foreach (IntervalBlockConfig intervalBlockConfig in meterReadingConfig.IntervalBlocks)
                    {
                        mr.IntervalBlocks.Add(CreateIntervalBlock(meterReadingConfig, intervalBlockConfig, hanConfig, usedValue, taf1Reg));
                    }
                    usedValue = rest;
                    rest = RandomNumber(0, usedValue);
                    usedValue = usedValue - rest;
                    tariffStageCounter--;
                }

                meterReadings.Add(mr);
            }
            return meterReadings;
        }

        private static IntervalBlock CreateIntervalBlockOML(
            MeterReadingConfig meterReadingConfig,
            IntervalBlockConfig intervalBlockConfig,
            HanAdapterExampleConfig hanConfig,
            int[] taf1Reg)
        {
            var intervalBlock = new IntervalBlock()
            {
                Interval = new Interval()
                {
                    Duration = intervalBlockConfig.Duration,
                    Start = intervalBlockConfig.Start.GetDateWithoutSeconds()
                }

            };

            int value = meterReadingConfig.OMLInitValue;
            var timestamp = intervalBlockConfig.Start;
            var denominator = intervalBlockConfig.Duration / meterReadingConfig.PeriodSeconds;
            double consumption = (double)(hanConfig.XmlConfig.ValueSummary / denominator);
            var index = 0;

            if (taf1Reg == null)
            {
                taf1Reg = new int[(int)denominator + 1];
            }

            while (timestamp <= intervalBlockConfig.Start.AddSeconds((uint)intervalBlockConfig.Duration).GetDateWithoutSeconds())
            {
                if(index == denominator)
                {
                    value = meterReadingConfig.OMLInitValue + hanConfig.XmlConfig.ValueSummary;
                }

                var ir = new IntervalReading()
                {
                    TimePeriod = new Interval()
                    {
                        Duration = 0,
                        Start = timestamp
                    },
                    Value = value
                };

                taf1Reg[index] = taf1Reg[index] + value;

                SetStatusWord(ir, intervalBlockConfig);
                timestamp = timestamp.AddSeconds((uint)meterReadingConfig.PeriodSeconds).GetDateWithoutSeconds();
                value = value + (int)consumption;
                intervalBlock.IntervalReadings.Add(ir);
                index++;
            }
            return intervalBlock;
        }

        private static IntervalBlock CreateIntervalBlock(
            MeterReadingConfig meterReadingConfig,
            IntervalBlockConfig intervalBlockConfig,
            HanAdapterExampleConfig hanConfig,
            int usedValue,
            int[] taf1Reg)
        {
            var intervalBlock = new IntervalBlock()
            {
                Interval = new Interval()
                {
                    Duration = intervalBlockConfig.Duration,
                    Start = intervalBlockConfig.Start.GetDateWithoutSeconds()
                }
            };

            if (hanConfig.Contract.TafId == TafId.Taf1)
            {
                if (taf1Reg == null)
                {
                    throw new InvalidOperationException("Das abgeleitete Register für Taf-1 wurde nicht befüllt.");
                }

                var timestamp = intervalBlockConfig.Start.GetDateWithoutSeconds();

                for (int i = 0; i < taf1Reg.Length; i++)
                {
                    var ir = new IntervalReading()
                    {
                        TimePeriod = new Interval()
                        {

                            Duration = 0,
                            Start = timestamp
                        },
                        Value = taf1Reg[i]
                    };
                    SetStatusWord(ir, intervalBlockConfig);
                    timestamp = timestamp.AddSeconds((uint)meterReadingConfig.PeriodSeconds).GetDateWithoutSeconds();
                    intervalBlock.IntervalReadings.Add(ir);
                }
            }
            else
            {
                var ir = new IntervalReading()
                {
                    TimePeriod = new Interval()
                    {
                        Duration = 0,
                        Start = intervalBlockConfig.Start.GetDateWithoutSeconds()
                    },
                    Value = usedValue
                };

                SetStatusWord(ir, intervalBlockConfig);
                intervalBlock.IntervalReadings.Add(ir);
            }

            return intervalBlock;
        }

        private static void SetStatusWord(IntervalReading ir, IntervalBlockConfig ibConfig)
        {
            if (ibConfig.UsedStatus == "FNN")
            {
                ir.StatusFNN = new StatusFNN(GetStatusFNN());
            }
            else if (ibConfig.UsedStatus == "PTB")
            {
                ir.StatusPTB = (StatusPTB)RandomNumber(0, 4);
            }
        }

        private static string BuildQualifiedLogicalName(string iD, string obisCode)
        {
            return $"{obisCode}.{iD}.sm";
        }

        private static string GetStatusFNN()
        {
            return "0000010500100504";
        }

        private static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max + 1);
        }

    }
}
