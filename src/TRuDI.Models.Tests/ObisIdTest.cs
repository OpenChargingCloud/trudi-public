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

        [TestMethod]
        public void TestObisId2()
        {
            var target = new ObisId("1-0:1.8.0*255");
            Assert.AreEqual(1, target.A);
            Assert.AreEqual(0, target.B);
            Assert.AreEqual(1, target.C);
            Assert.AreEqual(8, target.D);
            Assert.AreEqual(0, target.E);
            Assert.AreEqual(255, target.F);

            Assert.AreEqual("1-0:1.8.0*255", target.ToString());
        }

        [TestMethod]
        public void TestObisId3()
        {
            var target = new ObisId("1-0:1.8.0");
            Assert.AreEqual(1, target.A);
            Assert.AreEqual(0, target.B);
            Assert.AreEqual(1, target.C);
            Assert.AreEqual(8, target.D);
            Assert.AreEqual(0, target.E);
            Assert.AreEqual(255, target.F);

            Assert.AreEqual("1-0:1.8.0*255", target.ToString());
        }

        [TestMethod]
        public void TestObisIdEquals()
        {
            var a = new ObisId("1-0:1.8.0*255");
            var b = new ObisId("1-0:1.8.0*255");
            var c = new ObisId("1-0:1.8.1*255");

            Assert.IsTrue(a == b);
            Assert.IsFalse(a == c);
            Assert.IsTrue(a != c);
            Assert.IsFalse(a != b);

            Assert.IsTrue(a == "1-0:1.8.0*255");
            Assert.IsFalse(a == "1-0:1.8.1*255");

            Assert.IsTrue("1-0:1.8.0*255" == a);
            Assert.IsFalse("1-0:1.8.1*255" == a);
        }
    }
}
