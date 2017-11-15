namespace TRuDI.Models.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TRuDI.Models.BasicData;

    [TestClass]
    public class StatusFNNTests
    {
        [TestMethod]
        public void TestMapToStatusPtb()
        {
            var target = new StatusFNN("210500000004");
            Assert.AreEqual(StatusPTB.FatalError, target.MapToStatusPtb());

            target = new StatusFNN("200500000004");
            Assert.AreEqual(StatusPTB.TemporaryError, target.MapToStatusPtb());

            target = new StatusFNN("0500000004");
            Assert.AreEqual(StatusPTB.NoError, target.MapToStatusPtb());

            target = new StatusFNN("100500000004");
            Assert.AreEqual(StatusPTB.Warning, target.MapToStatusPtb());

            target = new StatusFNN("400500000004");
            Assert.AreEqual(StatusPTB.CriticalTemporaryError, target.MapToStatusPtb());
        }
    }
}
