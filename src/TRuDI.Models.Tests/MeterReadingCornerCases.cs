namespace TRuDI.Models.Tests
{

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Xml.Linq;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.Models;

    [TestClass]
    public class MeterReadingCornerCases
    {
        [TestMethod]
        [DeploymentItem(@"Data\cc_2_2_first_or_last_read_off_by_more_than_60_seconds.xml")]
        public void CC_2_2_FirstOrLastReadOffByMoreThan60Seconds()
        {
            var xml = XDocument.Load(@"Data\cc_2_2_first_or_last_read_off_by_more_than_60_seconds.xml");

            var ctx = new AdapterContext()
            {
                Start = new DateTime(2017, 7, 1, 0, 0, 0),
                End = new DateTime(2017, 8, 1, 0, 0, 0),
                Contract = new ContractInfo() { TafId = TafId.Taf7 }
            };

            Assert.IsTrue(RunValidations(xml, ctx));
        }

        [TestMethod]
        [DeploymentItem(@"Data\cc_2_3_reading_gaps_at_the_begin_of_ctx_period.xml")]
        public void CC_2_3_ReadingGapsAtTheBeginOfCtxPeriod()
        {
            var xml = XDocument.Load(@"Data\cc_2_3_reading_gaps_at_the_begin_of_ctx_period.xml");

            var ctx = new AdapterContext()
            {
                Start = new DateTime(2017, 9, 8, 0, 0, 0),
                End = new DateTime(2017, 9, 10, 0, 0, 0),
                Contract = new ContractInfo() { TafId = TafId.Taf7 }
            };

            Assert.IsTrue(RunValidations(xml, ctx));
        }

        [TestMethod]
        [DeploymentItem(@"Data\cc_2_4_reading_gaps_at_the_end_of_ctx_period.xml")]
        public void CC_2_4_ReadingGapsAtTheEndOfCtxPeriod()
        {
            var xml = XDocument.Load(@"Data\cc_2_4_reading_gaps_at_the_end_of_ctx_period.xml");

            var ctx = new AdapterContext()
            {
                Start = new DateTime(2017, 9, 10, 0, 0, 0),
                End = new DateTime(2017, 9, 18, 14, 0, 0),
                Contract = new ContractInfo() { TafId = TafId.Taf7 }
            };

            Assert.IsTrue(RunValidations(xml, ctx));
        }

        [TestMethod]
        [DeploymentItem(@"Data\cc_2_5_reading_gaps_inside_loaded_period.xml")]
        public void CC_2_5_ReadingGapsInsideLoadedPeriod()
        {
            var xml = XDocument.Load(@"Data\cc_2_5_reading_gaps_inside_loaded_period.xml");

            var ctx = new AdapterContext()
            {
                Start = new DateTime(2017, 9, 5, 0, 0, 0),
                End = new DateTime(2017, 9, 9, 14, 0, 0),
                Contract = new ContractInfo() { TafId = TafId.Taf7 }
            };

            Assert.IsTrue(RunValidations(xml, ctx));
        }

        [TestMethod]
        [DeploymentItem(@"Data\cc_3_2_query_over_multiple_bill_periods.xml")]
        public void CC_3_2_QueryOverMultipleBillPeriods()
        {
            var xml = XDocument.Load(@"Data\cc_3_2_query_over_multiple_bill_periods.xml");

            var ctx = new AdapterContext()
            {
                Start = new DateTime(2017, 9, 1, 0, 0, 0),
                End = new DateTime(2017, 9, 8, 14, 0, 0),
                Contract = new ContractInfo() { TafId = TafId.Taf6 }
            };
            ctx.BillingPeriod = new BillingPeriod() { Begin = ctx.Start, End = ctx.End };

            Assert.IsTrue(RunValidations(xml, ctx));
        }

        private bool RunValidations(XDocument xml, AdapterContext ctx)
        {
            try
            {
                Ar2418Validation.ValidateSchema(xml);

                var model = XmlModelParser.ParseHanAdapterModel(xml.Root?.Descendants());

                ModelValidation.ValidateHanAdapterModel(model);

                ContextValidation.ValidateContext(model, ctx);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
