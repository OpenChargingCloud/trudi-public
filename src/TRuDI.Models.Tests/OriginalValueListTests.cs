namespace TRuDI.Models.Tests
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using TRuDI.Models;

    [TestClass]
    public class OriginalValueListTests
    {
        [TestMethod]
        [DeploymentItem(@"Data\IF_Adapter_TRuDI_DatenTAF2.xml")]
        public void TestIsOriginalValueListTaf2()
        {
            var xml = XDocument.Load(@"Data\IF_Adapter_TRuDI_DatenTAF2.xml");
            var model = XmlModelParser.ParseHanAdapterModel(xml.Root.Descendants());

            var target = new OriginalValueList(model.MeterReadings[0]);
        }

        [TestMethod]
        [DeploymentItem(@"Data\IF_Adapter_TRuDI_DatenTAF7.xml")]
        public void TestIsOriginalValueListTaf7()
        {
            var xml = XDocument.Load(@"Data\IF_Adapter_TRuDI_DatenTAF7.xml");
            var model = XmlModelParser.ParseHanAdapterModel(xml.Root.Descendants());

            var target = new OriginalValueList(model.MeterReadings[0]);

            Assert.AreEqual("1-0:1.8.0*255", target.Obis.ToString());
            Assert.AreEqual(0, target.GapCount);

            Assert.AreEqual(DateTime.Parse("2017-06-26T11:30:00+02:00"), target.Start);
            Assert.AreEqual(DateTime.Parse("2017-06-26T12:00:00+02:00"), target.End);

            var items = target.GetReadings(DateTime.MinValue, DateTime.MaxValue).ToList();
            Assert.AreEqual(3, items.Count);

            Assert.AreEqual(DateTime.Parse("2017-06-26T11:30:00+02:00"), items[0].TimePeriod.Start);
            Assert.AreEqual(DateTime.Parse("2017-06-26T11:45:00+02:00"), items[1].TimePeriod.Start);
            Assert.AreEqual(DateTime.Parse("2017-06-26T12:00:00+02:00"), items[2].TimePeriod.Start);
        }

        [TestMethod]
        [DeploymentItem(@"Data\IF_Adapter_TRuDI_DatenTAF7_With_Gaps.xml")]
        public void TestIsOriginalValueListTaf7WithGaps()
        {
            var xml = XDocument.Load(@"Data\IF_Adapter_TRuDI_DatenTAF7_With_Gaps.xml");
            var model = XmlModelParser.ParseHanAdapterModel(xml.Root.Descendants());

            var target = new OriginalValueList(model.MeterReadings[0]);

            Assert.AreEqual("1-0:1.8.0*255", target.Obis.ToString());
            Assert.AreEqual(3, target.GapCount);

            Assert.AreEqual(DateTime.Parse("2017-06-26T11:30:00+02:00"), target.Start);
            Assert.AreEqual(DateTime.Parse("2017-06-26T15:15:00+02:00"), target.End);

            // Get only 2 items
            var items = target.GetReadings(DateTime.Parse("2017-06-26T11:45:00+02:00"), DateTime.Parse("2017-06-26T12:00:00+02:00")).ToList();
            Assert.AreEqual(2, items.Count);

            Assert.AreEqual(DateTime.Parse("2017-06-26T11:45:00+02:00"), items[0].TimePeriod.Start);
            Assert.IsNotNull(items[0].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T12:00:00+02:00"), items[1].TimePeriod.Start);
            Assert.IsNotNull(items[1].Value);


            // Get all items
            items = target.GetReadings(DateTime.MinValue, DateTime.MaxValue).ToList();
            Assert.AreEqual(16, items.Count);

            Assert.AreEqual(DateTime.Parse("2017-06-26T11:30:00+02:00"), items[0].TimePeriod.Start);
            Assert.IsNotNull(items[0].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T11:45:00+02:00"), items[1].TimePeriod.Start);
            Assert.IsNotNull(items[1].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T12:00:00+02:00"), items[2].TimePeriod.Start);
            Assert.IsNotNull(items[2].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T12:15:00+02:00"), items[3].TimePeriod.Start);
            Assert.IsNull(items[3].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T12:30:00+02:00"), items[4].TimePeriod.Start);
            Assert.IsNotNull(items[4].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T12:45:00+02:00"), items[5].TimePeriod.Start);
            Assert.IsNotNull(items[5].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T13:00:00+02:00"), items[6].TimePeriod.Start);
            Assert.IsNotNull(items[6].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T13:15:00+02:00"), items[7].TimePeriod.Start);
            Assert.IsNull(items[7].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T13:30:00+02:00"), items[8].TimePeriod.Start);
            Assert.IsNull(items[8].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T13:45:00+02:00"), items[9].TimePeriod.Start);
            Assert.IsNotNull(items[9].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T14:00:00+02:00"), items[10].TimePeriod.Start);
            Assert.IsNull(items[10].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T14:15:00+02:00"), items[11].TimePeriod.Start);
            Assert.IsNull(items[11].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T14:30:00+02:00"), items[12].TimePeriod.Start);
            Assert.IsNull(items[12].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T14:45:00+02:00"), items[13].TimePeriod.Start);
            Assert.IsNull(items[13].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T15:00:00+02:00"), items[14].TimePeriod.Start);
            Assert.IsNotNull(items[14].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T15:15:00+02:00"), items[15].TimePeriod.Start);
            Assert.IsNotNull(items[15].Value);
            
            // Get only gap items
            items = target.GetReadings(DateTime.Parse("2017-06-26T14:15:00+02:00"), DateTime.Parse("2017-06-26T14:30:00+02:00")).ToList();
            Assert.AreEqual(2, items.Count);

            Assert.AreEqual(DateTime.Parse("2017-06-26T14:15:00+02:00"), items[0].TimePeriod.Start);
            Assert.IsNull(items[0].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T14:30:00+02:00"), items[1].TimePeriod.Start);
            Assert.IsNull(items[1].Value);

            // Get items with gap at beginn
            items = target.GetReadings(DateTime.Parse("2017-06-26T14:45:00+02:00"), DateTime.Parse("2017-06-26T15:15:00+02:00")).ToList();
            Assert.AreEqual(3, items.Count);

            Assert.AreEqual(DateTime.Parse("2017-06-26T14:45:00+02:00"), items[0].TimePeriod.Start);
            Assert.IsNull(items[0].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T15:00:00+02:00"), items[1].TimePeriod.Start);
            Assert.IsNotNull(items[1].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T15:15:00+02:00"), items[2].TimePeriod.Start);
            Assert.IsNotNull(items[2].Value);

            // Get items with gap at end
            items = target.GetReadings(DateTime.Parse("2017-06-26T13:45:00+02:00"), DateTime.Parse("2017-06-26T14:15:00+02:00")).ToList();
            Assert.AreEqual(3, items.Count);

            Assert.AreEqual(DateTime.Parse("2017-06-26T13:45:00+02:00"), items[0].TimePeriod.Start);
            Assert.IsNotNull(items[0].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T14:00:00+02:00"), items[1].TimePeriod.Start);
            Assert.IsNull(items[1].Value);

            Assert.AreEqual(DateTime.Parse("2017-06-26T14:15:00+02:00"), items[2].TimePeriod.Start);
            Assert.IsNull(items[2].Value);
        }
    }
}
