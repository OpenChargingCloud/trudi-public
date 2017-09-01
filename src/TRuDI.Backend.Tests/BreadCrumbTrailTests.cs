namespace TRuDI.Backend.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using TRuDI.Backend.Application;

    [TestClass]
    public class BreadCrumbTrailTests
    {
        [TestMethod]
        public void TestBreadCrumbTrail()
        {
            var target = new BreadCrumbTrail();

            target.Add("A", "A");
            target.Add("B", "B");

            target.BackTo(1);
            Assert.AreEqual(2, target.Items.Count);

            target.BackTo(0);
            Assert.AreEqual(1, target.Items.Count);
            Assert.AreEqual("A", target.Items[0].Name);
        }
    }
}
