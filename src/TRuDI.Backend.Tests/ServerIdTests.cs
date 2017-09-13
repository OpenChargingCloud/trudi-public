namespace TRuDI.Backend.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using TRuDI.Models;

    [TestClass]
    public class ServerIdTests
    {
        [TestMethod]
        public void TestServerId1()
        {
            var target = new ServerId("EPPC0210486901");
            Assert.AreEqual(ObisMedium.Communication, target.Medium);
            Assert.AreEqual("PPC", target.FlagId);
            Assert.AreEqual(2, target.ProductionBlock);
            Assert.AreEqual(10486901u, target.Number);

            Assert.AreEqual("E PPC 02 10486901", target.ToString());
            Assert.AreEqual("0A0E5050430200A00475", target.ToHexString());
        }

        [TestMethod]
        public void TestServerId2()
        {
            var target = new ServerId("0A01454D48000051971E");
            Assert.AreEqual(ObisMedium.Electricity, target.Medium);
            Assert.AreEqual("EMH", target.FlagId);
            Assert.AreEqual(0, target.ProductionBlock);
            Assert.AreEqual(5347102u, target.Number);

            Assert.AreEqual("1 EMH 00 05347102", target.ToString());
            Assert.AreEqual("0A01454D48000051971E", target.ToHexString());
        }
    }
}
