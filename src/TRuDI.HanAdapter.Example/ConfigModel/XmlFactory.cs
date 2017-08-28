namespace TRuDI.HanAdapter.Example.ConfigModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;
    using TRuDI.HanAdapter.XmlValidation.Models.CheckData;

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

            var meterReadings = MeterReadingCreator(hanConfig);

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
                            intervalReading.Add(new XElement(ar + "statusPTB", (byte)iReading.StatusPTB));
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

            if(hanConfig.Contract.TafId == TafId.Taf7)
            {
                hanConfig.SupplierXml = FabricateLieferantXml(hanConfig);
            }

            return trudiXml;
        }
       
        private static XDocument FabricateLieferantXml(HanAdapterExampleConfig hanConfig)
        {
            XNamespace ar = XNamespace.Get("http://vde.de/AR_2418-6.xsd");
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace schemaLocation = XNamespace.Get("http://vde.de/AR_2418-6.xsd AR_2418-6.xsd");
            XNamespace espi = XNamespace.Get("http://naesb.org/espi");
            XNamespace atom = XNamespace.Get("http://www.w3.org/2005/Atom");

            XElement serviceCategory = new XElement(espi + "ServiceCategory", new XElement(espi + "kind", hanConfig.XmlConfig.ServiceCategoryKind));

            XElement invoicingParty = new XElement(ar + "InvoicingParty", new XElement(ar + "invoicingPartyId", hanConfig.XmlConfig.InvoicingPartyId));

            XElement smgw = new XElement(ar + "SMGW");

            smgw.Add(new XElement(ar + "smgwId", hanConfig.XmlConfig.SmgwId));

            XElement usagePoint = new XElement(ar + "UsagePoint",
                                               serviceCategory,
                                               new XElement(ar + "usagePointId", hanConfig.XmlConfig.UsagePointId));

            if (hanConfig.XmlConfig.CustomerId != null)
            {
                XElement customer = new XElement(ar + "Customer", new XElement(ar + "customerId", hanConfig.XmlConfig.CustomerId));
                usagePoint.Add(customer);
            }

            usagePoint.Add(invoicingParty, smgw);

            usagePoint.Add(new XElement(ar + "tariffName", hanConfig.XmlConfig.TariffName));

            XElement analysisProfile = new XElement(ar + "AnalysisProfile",
                                                    new XElement(ar + "tariffUseCase", hanConfig.XmlConfig.TariffUseCase),
                                                    new XElement(ar + "tariffId", hanConfig.XmlConfig.TariffName),
                                                    new XElement(ar + "billingPeriod",
                                                     new XElement(ar + "duration", (int)(hanConfig.BillingPeriod.End - hanConfig.BillingPeriod.Begin)?.TotalSeconds),
                                                     new XElement(ar + "start", hanConfig.BillingPeriod.Begin) 
                                                    ));

            foreach (TariffStageConfig config in hanConfig.XmlConfig.TariffStageConfigs)
            {
                XElement tariffStage = new XElement(ar + "TariffStage", new XElement(ar + "tariffNumber", config.TariffNumber));

                if(config.Description != null)
                {
                    tariffStage.Add(new XElement(ar + "description", config.Description));
                }

                tariffStage.Add(new XElement(ar + "obisCode", config.ObisCode));

                analysisProfile.Add(tariffStage);
            }

            analysisProfile.Add(new XElement(ar + "defaultTariffNumber", hanConfig.XmlConfig.DefaultTariffNumber));

            XElement tariffChangeTrigger = new XElement(ar + "TariffChangeTrigger");

            XElement timeTrigger = new XElement(ar + "TimeTrigger");

            var dayProfiles = DayProfileCreator(hanConfig);

            foreach(DayProfile profile in dayProfiles)
            {
                XElement dayProfile = new XElement(ar + "DayProfile", new XElement(ar + "dayId", profile.DayId));

                foreach(DayTimeProfile dtProfile in profile.DayTimeProfiles)
                {
                    XElement dayTimeProfile = new XElement(ar + "DayTimeProfile");

                    XElement startTime = new XElement(ar + "startTime", 
                        new XElement(ar + "hour", dtProfile.StartTime.Hour),
                        new XElement(ar + "minute", dtProfile.StartTime.Minute));

                    dayTimeProfile.Add(startTime, new XElement(ar + "tariffNumber", dtProfile.TariffNumber));

                    dayProfile.Add(dayTimeProfile);
                }

                timeTrigger.Add(dayProfile);
            }

            var specialDayProfiles = SpecialDayProfileCreator(hanConfig);

            foreach(SpecialDayProfile profile in specialDayProfiles)
            {
                XElement specialDayProfile = new XElement(ar + "SpecialDayProfile");

                specialDayProfile.Add(new XElement(ar + "specialDayDate", 
                                                    new XElement(ar + "year", profile.SpecialDayDate.Year),
                                                    new XElement(ar + "month", profile.SpecialDayDate.Month),
                                                    new XElement(ar + "day_of_month", profile.SpecialDayDate.DayOfMonth)));

                specialDayProfile.Add(new XElement(ar + "dayId", profile.DayId));

                timeTrigger.Add(specialDayProfile);  
            }
           
            tariffChangeTrigger.Add(timeTrigger);

            analysisProfile.Add(tariffChangeTrigger);

            usagePoint.Add(analysisProfile);

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

                        mr.IntervalBlocks.Add(CreateIntervalBlockOML(meterReadingConfig, intervalBlockConfig, hanConfig));
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
                        mr.IntervalBlocks.Add(CreateIntervalBlock(meterReadingConfig, intervalBlockConfig, hanConfig, usedValue));
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
            HanAdapterExampleConfig hanConfig)
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

            if (hanConfig.XmlConfig.Taf1Reg == null)
            {
                hanConfig.XmlConfig.Taf1Reg = new int[(int)denominator];
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

                if (index < denominator)
                {
                    hanConfig.XmlConfig.Taf1Reg[index] = hanConfig.XmlConfig.Taf1Reg[index] + (int)consumption;
                }
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
            int usedValue)
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
                if (hanConfig.XmlConfig.Taf1Reg == null)
                {
                    throw new InvalidOperationException("Das abgeleitete Register für Taf-1 wurde nicht befüllt.");
                }

                var timestamp = intervalBlockConfig.Start.GetDateWithoutSeconds().AddMonths(1);

                for (int i = 0; i < hanConfig.XmlConfig.Taf1Reg.Length; i++)
                {
                    var ir = new IntervalReading()
                    {
                        TimePeriod = new Interval()
                        {

                            Duration = 0,
                            Start = timestamp
                        },
                        Value = hanConfig.XmlConfig.Taf1Reg[i]
                    };
                    SetStatusWord(ir, intervalBlockConfig);
                    timestamp = timestamp.AddMonths(1);
                        //AddSeconds((uint)meterReadingConfig.PeriodSeconds).GetDateWithoutSeconds();
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

        private static List<DayProfile> DayProfileCreator(HanAdapterExampleConfig hanConfig)
        {
            var dayProfiles = new List<DayProfile>();

            foreach(DayProfileConfig config in hanConfig.XmlConfig.DayProfiles)
            {
                var dayProfile = new DayProfile();
                var dayTimeProfiles = new List<DayTimeProfile>();

                dayProfile.DayId = config.DayId;

                foreach (DayTimeProfileConfig dayTimeConfig in config.DayTimeProfiles)
                {
                    var time = dayTimeConfig.Start;
                    
                    while(time <= dayTimeConfig.End)
                    {
                        var dayTimeProfile = new DayTimeProfile();
                        dayTimeProfile.StartTime.Hour = (byte)time.Hour;
                        dayTimeProfile.StartTime.Minute = (byte)time.Minute;
                        dayTimeProfile.TariffNumber = dayTimeConfig.TariffNumber;
                        time = time.AddSeconds(900);
                        dayTimeProfiles.Add(dayTimeProfile);
                    }

                }

                dayProfile.DayTimeProfiles = dayTimeProfiles;
                dayProfiles.Add(dayProfile);
            }

            return dayProfiles;
        }

        private static List<SpecialDayProfile> SpecialDayProfileCreator(HanAdapterExampleConfig hanConfig)
        {
            var specialDayProfiles = new List<SpecialDayProfile>();

            var date = hanConfig.BillingPeriod.Begin;

            while(date < hanConfig.BillingPeriod.End)
            {
                var specialDayProfile = new SpecialDayProfile();
                specialDayProfile.SpecialDayDate = new DayVarType();
                specialDayProfile.SpecialDayDate.Year = (ushort)date.Year;
                specialDayProfile.SpecialDayDate.Month = (byte)date.Month;
                specialDayProfile.SpecialDayDate.DayOfMonth = (byte)date.Day;
                if(hanConfig.XmlConfig.DayIdCount == 2)
                {
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        specialDayProfile.DayId = 2;
                    }
                    else
                    {
                        specialDayProfile.DayId = 1;
                    }
                }
                else
                {
                    specialDayProfile.DayId = (ushort)RandomNumber(1, hanConfig.XmlConfig.DayIdCount);
                }
                date = date.AddDays(1);
                specialDayProfiles.Add(specialDayProfile);
            }
            return specialDayProfiles;
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
