namespace TRuDI.TafAdapter.Taf2.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;
    using TRuDI.TafAdapter.Interface;

    [TestClass]
    public class TafAdapterTaf2Tests
    {
        [TestMethod]
        public void TestAddAmountToDayRegister()
        {
            var reg181 = new Register() { ObisCode = new ObisId("0100010801FF"), Amount = 0, TariffId = 1 };
            var reg182 = new Register() { ObisCode = new ObisId("0100010802FF"), Amount = 0, TariffId = 2 };
            var reg18x = new Register() { ObisCode = new ObisId("010001083FFF"), Amount = 0, TariffId = 63 };

            var target = new AccountingDay(new[] { reg181, reg182, reg18x });

            target.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 1, Amount = 25 });
            target.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 2, Amount = 25 });
            target.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 63, Amount = 25 });
           


            Assert.AreEqual(25, reg181.Amount);
            Assert.AreEqual(25, reg182.Amount);
            Assert.AreEqual(25, reg18x.Amount);

            target.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 1, Amount = 5 });
            target.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 2, Amount = 5 });
            target.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 63, Amount = 5 });

            Assert.AreEqual(30, reg181.Amount);
            Assert.AreEqual(30, reg182.Amount);
            Assert.AreEqual(30, reg18x.Amount);

            target.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 1, Amount = 15 });
            target.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 2, Amount = 15 });
            target.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 63, Amount = 15 });


            Assert.AreEqual(45, reg181.Amount);
            Assert.AreEqual(45, reg182.Amount);
            Assert.AreEqual(45, reg18x.Amount);

            Assert.AreEqual(9, target.MeasuringRanges.Count);
        }

        [TestMethod]
        public void TestAddAmountToPeriodRange()
        {
            var reg181 = new Register() { ObisCode = new ObisId("0100010801FF"), Amount = 0, TariffId = 1 };
            var reg182 = new Register() { ObisCode = new ObisId("0100010802FF"), Amount = 0, TariffId = 2 };
            var reg18x = new Register() { ObisCode = new ObisId("010001083FFF"), Amount = 0, TariffId = 63 };

            var dayReg181 = new Register() { ObisCode = new ObisId("010001083FFF"), Amount = 0, TariffId = 1 };
            var dayReg182 = new Register() { ObisCode = new ObisId("010001083FFF"), Amount = 0, TariffId = 2 };
            var dayReg18x = new Register() { ObisCode = new ObisId("010001083FFF"), Amount = 0, TariffId = 63 };

            var day = new AccountingDay(new[] { dayReg181, dayReg182, dayReg18x });

            var target = new AccountingPeriod(new[] { reg181, reg182, reg18x });

            day.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 1, Amount = 50 });
            day.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 2, Amount = 25 });
            day.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 63, Amount = 10 });

            target.Add(day);

            Assert.AreEqual(50, reg181.Amount);
            Assert.AreEqual(25, reg182.Amount);
            Assert.AreEqual(10, reg18x.Amount);

            day.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 1, Amount = 25 });
            day.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 2, Amount = 15 });
            day.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 63, Amount = 5 });

            target.Add(day);

            Assert.AreEqual(125, reg181.Amount);
            Assert.AreEqual(65, reg182.Amount);
            Assert.AreEqual(25, reg18x.Amount);

            day.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 1, Amount = 25 });
            day.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 2, Amount = 20 });
            day.Add(new MeasuringRange() { Start = DateTime.Now.AddMonths(-1), End = DateTime.Now, TariffId = 63, Amount = 5 });

            target.Add(day);

            Assert.AreEqual(225, reg181.Amount);
            Assert.AreEqual(125, reg182.Amount);
            Assert.AreEqual(45, reg18x.Amount);

            Assert.AreEqual(3, target.AccountingSections.Count);
        }

        [TestMethod]
        public void TestMeasuringRangeKonstruktorWithMeterReading()
        {
            var mrObis163 = new MeterReading
            {
                ReadingType = new ReadingType()
                {
                    ObisCode = "010001083FFF"
                }
            };

            var mrObis263 = new MeterReading
            {
                ReadingType = new ReadingType()
                {
                    ObisCode = "010002083FFF"
                }
            };

            var mrObis63 = new MeterReading
            {
                ReadingType = new ReadingType()
                {
                    ObisCode = "010000083FFF"
                }
            };

            var target = new MeasuringRange(DateTime.Now.AddMonths(-1), DateTime.Now, mrObis163, 1);

            Assert.AreEqual(63, target.TariffId);

            target = new MeasuringRange(DateTime.Now.AddMonths(-1), DateTime.Now, mrObis263, 1);

            Assert.AreEqual(63, target.TariffId);

            target = new MeasuringRange(DateTime.Now.AddMonths(-1), DateTime.Now, mrObis63, 1);

            Assert.AreEqual(63, target.TariffId);

            var code = "010002083FFF";
            for (int i = 3; i < 256; i++)
            {
                var obisC = i.ToString("X2");
                var obisCode = code.Substring(0, 4) + obisC + code.Substring(6);

                mrObis63.ReadingType.ObisCode = obisCode;
                target = new MeasuringRange(DateTime.Now.AddMonths(-1), DateTime.Now, mrObis63, 1);
                Assert.AreEqual(63, target.TariffId);
            }
        }

        [TestMethod]
        public void TestSetIntervalReading()
        {
            var adapter = new TafAdapterTaf2();
            var reading = new MeterReading();

            var irSet1 = new List<IntervalReading>
            {
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 1), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 1, 0, 15, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 1, 0, 30, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 1, 0, 45, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 1, 1, 0, 0), Duration = 900 } },

                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 15, 12, 0, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 15, 12, 15, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 15, 12, 30, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 15, 12, 45, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 15, 13, 0, 0), Duration = 900 } },

                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 31, 23, 0, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 31, 23, 15, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 31, 23, 30, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 1, 31, 23, 45, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 1), Duration = 900 } }
            };

            reading.IntervalBlocks.Add(new IntervalBlock()
            {
                Interval = new Interval()
                {
                    Start = new DateTime(2017, 1, 1),
                    Duration = 2678400
                },
                IntervalReadings = irSet1
            });

            var irSet2 = new List<IntervalReading>
            {
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 1), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 1, 0, 15, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 1, 0, 30, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 1, 0, 45, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 1, 1, 0, 0), Duration = 900 } },

                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 15, 12, 0, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 15, 12, 15, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 15, 12, 30, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 15, 12, 45, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 15, 13, 0, 0), Duration = 900 } },

                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 28, 23, 0, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 28, 23, 15, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 28, 23, 30, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 2, 28, 23, 45, 0), Duration = 900 } },
                new IntervalReading() { TimePeriod = new Interval() { Start = new DateTime(2017, 3, 1), Duration = 900 } }
            };

            reading.IntervalBlocks.Add(new IntervalBlock()
            {
                Interval = new Interval()
                {
                    Start = new DateTime(2017, 2, 1),
                    Duration = 2419200
                },
                IntervalReadings = irSet2
            });

            // Testing time periods incloded in meterReading.IntervalBlocks[0]
            var date = new DateTime(2017, 1, 1);
            var ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 1, 0, 15, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 1, 0, 30, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 1, 0, 45, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 1, 1, 0, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 15, 12, 0, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 15, 12, 15, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 15, 12, 30, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 15, 12, 45, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 15, 13, 0, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 31, 23, 0, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 31, 23, 15, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 31, 23, 30, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 1, 31, 23, 45, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                if(index == 95)
                {
                    var result = adapter.SetIntervalReading(reading, date, index);
                    Assert.AreEqual(date.AddSeconds(900), result.end);
                    Assert.AreEqual(ir.TimePeriod.Start.AddSeconds(900), result.reading.TimePeriod.Start);
                }
                else
                {
                    var result = adapter.SetIntervalReading(reading, date, index);
                    Assert.AreEqual(date, result.end);
                    Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
                }
            }


            // Testing time periods incloded in meterReading.IntervalBlocks[1]
            date = new DateTime(2017, 2, 1);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 1, 0, 15, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 1, 0, 30, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 1, 0, 45, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 1, 1, 0, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 15, 12, 0, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 15, 12, 15, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 15, 12, 30, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 15, 12, 45, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 15, 13, 0, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 28, 23, 0, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 28, 23, 15, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 28, 23, 30, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }

            date = new DateTime(2017, 2, 28, 23, 45, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                if (index == 95)
                {
                    var result = adapter.SetIntervalReading(reading, date, index);
                    Assert.AreEqual(date.AddSeconds(900), result.end);
                    Assert.AreEqual(ir.TimePeriod.Start.AddSeconds(900), result.reading.TimePeriod.Start);
                }
                else
                {
                    var result = adapter.SetIntervalReading(reading, date, index);
                    Assert.AreEqual(date, result.end);
                    Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
                }
            }

            date = new DateTime(2017, 3, 1);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                var result = adapter.SetIntervalReading(reading, date, index);
                Assert.AreEqual(date, result.end);
                Assert.AreEqual(ir.TimePeriod.Start, result.reading.TimePeriod.Start);
            }


            // Testing time periods not incloded in meterReading.IntervalBlocks
            date = new DateTime(2017, 1, 28, 9, 0, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                Assert.AreEqual((null, date), adapter.SetIntervalReading(reading, date, index));
            }

            date = new DateTime(2017, 1, 13, 14, 30, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                Assert.AreEqual((null, date), adapter.SetIntervalReading(reading, date, index));
            }

            date = new DateTime(2017, 5, 5, 9, 0, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                Assert.AreEqual((null, date), adapter.SetIntervalReading(reading, date, index));
            }

            date = new DateTime(2017, 4, 28, 23, 45, 0);
            ir = new IntervalReading() { TimePeriod = new Interval() { Start = date, Duration = 900 } };
            for (int index = 0; index < 96; index++)
            {
                if (index == 95)
                {
                    Assert.AreEqual((null, date.AddSeconds(900)), adapter.SetIntervalReading(reading, date, index));
                }
                else
                {
                    Assert.AreEqual((null, date), adapter.SetIntervalReading(reading, date, index));
                }
            }
        }
        
        public void TestFindNextValidTime()
        {
            throw new NotImplementedException();
        }

        public void TestFindLastValidTime()
        {
            throw new NotImplementedException();
        }

        public void TestGetDayData()
        {
            throw new NotImplementedException();
        }

        public void TestGetValidDayProfilesForMeterReading()
        {
            throw new NotImplementedException();
        }
    }
}
