namespace TRuDI.Models.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TRuDI.HanAdapter.XmlValidation.Models;

    [TestClass]
    public class ObisIdTests
    {
        [TestMethod]
        public void TestObisId()
        {
            var target = new ObisId("0100010800ff");
            Assert.AreEqual(1, target.A);
            Assert.AreEqual(0, target.B);
            Assert.AreEqual(1, target.C);
            Assert.AreEqual(8, target.D);
            Assert.AreEqual(0, target.E);
            Assert.AreEqual(255, target.F);

            Assert.AreEqual("1-0:1.8.0*255", target.ToString());
        }
    }
}
