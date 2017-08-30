

namespace TRuDI.TafAdapter.Taf2.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.TafAdapter.Interface;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestAddAmountToRegister()
        {
            var reg181 = new Register() { ObisCode = new ObisId("0100010801FF"), Amount = 0, TariffId = 1 };
            var reg182 = new Register() { ObisCode = new ObisId("0100010802FF"), Amount = 0, TariffId = 2 };
            var reg18x = new Register() { ObisCode = new ObisId("010001083FFF"), Amount = 0, TariffId = 63 };

            var target = new AccountingDay(new[] { reg181, reg182, reg18x });

            //target.Add(new MeasuringRange() { });

            Assert.AreEqual(0, reg181.Amount);
            Assert.AreEqual(20, reg182.Amount);
            Assert.AreEqual(0, reg18x.Amount);
            
            target.Add(null);

            Assert.AreEqual(5, reg181.Amount);
            Assert.AreEqual(20, reg182.Amount);
            Assert.AreEqual(0, reg18x.Amount);

            target.Add(null);

            Assert.AreEqual(5, reg181.Amount);
            Assert.AreEqual(20, reg182.Amount);
            Assert.AreEqual(5, reg18x.Amount);

            Assert.AreEqual(3, target.MeasuringRanges.Count);


        }
    }
}
