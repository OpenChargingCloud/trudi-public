namespace TRuDI.Models.Tests
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using TRuDI.Models;

    [TestClass]
    public class MeterReadingExtensionsTests
    {
        [TestMethod]
        [DeploymentItem(@"Data\IF_Adapter_TRuDI_DatenTAF2.xml")]
        public void TestIsOriginalValueList()
        {
            var xml = XDocument.Load(@"Data\IF_Adapter_TRuDI_DatenTAF2.xml");
            var model = XmlModelParser.ParseHanAdapterModel(xml.Root.Descendants());

            Assert.IsTrue(model.MeterReadings[0].IsOriginalValueList());
            Assert.IsFalse(model.MeterReadings[1].IsOriginalValueList());
        }

        [TestMethod]
        [DeploymentItem(@"Data\IF_Adapter_TRuDI_DatenTAF2.xml")]
        public void TestGetMeasurementPeriodTaf2()
        {
            var xml = XDocument.Load(@"Data\IF_Adapter_TRuDI_DatenTAF2.xml");
            var model = XmlModelParser.ParseHanAdapterModel(xml.Root.Descendants());

            Assert.AreEqual(TimeSpan.Zero, model.MeterReadings[0].GetMeasurementPeriod());
        }

        [TestMethod]
        [DeploymentItem(@"Data\IF_Adapter_TRuDI_DatenTAF7.xml")]
        public void TestGetMeasurementPeriodTaf7()
        {
            var xml = XDocument.Load(@"Data\IF_Adapter_TRuDI_DatenTAF7.xml");
            var model = XmlModelParser.ParseHanAdapterModel(xml.Root.Descendants());

            Assert.AreEqual(TimeSpan.FromMinutes(15), model.MeterReadings[0].GetMeasurementPeriod());
        }

        [TestMethod]
        [DeploymentItem(@"Data\IF_Adapter_TRuDI_DatenTAF7.xml")]
        public void TestGetGapCountTaf7()
        {
            var xml = XDocument.Load(@"Data\IF_Adapter_TRuDI_DatenTAF7.xml");
            var model = XmlModelParser.ParseHanAdapterModel(xml.Root.Descendants());

            Assert.AreEqual(0, model.MeterReadings[0].IntervalBlocks.FirstOrDefault().GetGapCount(TimeSpan.FromMinutes(15)));
        }

        [TestMethod]
        [DeploymentItem(@"Data\IF_Adapter_TRuDI_DatenTAF7_With_Gaps.xml")]
        public void TestGetGapCountTaf7WithGaps()
        {
            var xml = XDocument.Load(@"Data\IF_Adapter_TRuDI_DatenTAF7_With_Gaps.xml");
            var model = XmlModelParser.ParseHanAdapterModel(xml.Root.Descendants());

            Assert.AreEqual(3, model.MeterReadings[0].IntervalBlocks.FirstOrDefault().GetGapCount(TimeSpan.FromMinutes(15)));
        }
    }
}
