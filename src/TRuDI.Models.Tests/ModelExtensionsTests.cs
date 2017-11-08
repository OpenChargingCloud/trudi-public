namespace TRuDI.Models.Tests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TRuDI.Models;
    using TRuDI.Models.BasicData;

    [TestClass]
    public class ModelExtensionsTests
    {
        [TestMethod]
        public void TestGetSmoothCaptureTime()
        {
            var expected = new DateTime(2017, 1, 1, 10, 15, 0, DateTimeKind.Local);
            var actual = ModelExtensions.GetSmoothCaptureTime(new DateTime(2017, 1, 1, 10, 15, 0, DateTimeKind.Local));
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Kind, actual.Kind);

            expected = new DateTime(2017, 1, 1, 10, 15, 0, DateTimeKind.Utc);
            actual = ModelExtensions.GetSmoothCaptureTime(new DateTime(2017, 1, 1, 10, 15, 7, DateTimeKind.Utc));
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Kind, actual.Kind);

            expected = new DateTime(2017, 1, 1, 10, 15, 0, DateTimeKind.Local);
            actual = ModelExtensions.GetSmoothCaptureTime(new DateTime(2017, 1, 1, 10, 14, 53, DateTimeKind.Local));
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Kind, actual.Kind);

            expected = new DateTime(2017, 1, 1, 10, 15, 0, DateTimeKind.Local);
            actual = ModelExtensions.GetSmoothCaptureTime(new DateTime(2017, 1, 1, 10, 15, 9, DateTimeKind.Local));
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Kind, actual.Kind);

            expected = new DateTime(2017, 1, 1, 10, 15, 0, DateTimeKind.Local);
            actual = ModelExtensions.GetSmoothCaptureTime(new DateTime(2017, 1, 1, 10, 14, 51, DateTimeKind.Local));
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Kind, actual.Kind);

            expected = new DateTime(2017, 1, 1, 10, 0, 0, DateTimeKind.Local);
            actual = ModelExtensions.GetSmoothCaptureTime(new DateTime(2017, 1, 1, 10, 0, 18, DateTimeKind.Local), 3600);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Kind, actual.Kind);

            expected = new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Local);
            actual = ModelExtensions.GetSmoothCaptureTime(new DateTime(2017, 1, 1, 0, 12, 0, DateTimeKind.Local), 86400);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Kind, actual.Kind);

            expected = new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Local);
            actual = ModelExtensions.GetSmoothCaptureTime(new DateTime(2016, 12, 31, 23, 53, 5, DateTimeKind.Local), 86400);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Kind, actual.Kind);
        }

        [TestMethod]
        public void TestIsValidMeasurementPeriodTimestamp()
        {
            Assert.IsTrue(ModelExtensions.IsValidMeasurementPeriodTimestamp(new DateTime(2016, 12, 31, 23, 53, 5, DateTimeKind.Local), 86400));
            Assert.IsFalse(ModelExtensions.IsValidMeasurementPeriodTimestamp(new DateTime(2016, 12, 31, 23, 40, 5, DateTimeKind.Local), 86400));
            Assert.IsTrue(ModelExtensions.IsValidMeasurementPeriodTimestamp(new DateTime(2016, 12, 31, 17, 45, 5, DateTimeKind.Local), 900));
        }

        /// <summary>
        /// testing the extension on days when DST change happens
        /// </summary>
        [TestMethod]
        public void TestIntervalGetEnd()
        {
            Interval _interval = new Interval() { Start = new DateTime(2017, 3, 26, 0, 0, 0, DateTimeKind.Local), Duration = 3600 * 6 };

            Assert.IsTrue(_interval.GetEnd().Kind == DateTimeKind.Local);
            Assert.IsTrue(_interval.GetEnd().Hour == 7);
            Assert.IsTrue(_interval.GetEnd().Minute == 0);
            Assert.IsTrue(_interval.GetEnd().IsDaylightSavingTime());

            _interval = new Interval() { Start = new DateTime(2017, 10, 29, 0, 0, 0, DateTimeKind.Unspecified), Duration = 3600 * 6 };

            Assert.IsTrue(_interval.GetEnd().Kind == DateTimeKind.Local);
            Assert.IsTrue(_interval.GetEnd().Hour == 5);
            Assert.IsTrue(_interval.GetEnd().Minute == 0);
            Assert.IsFalse(_interval.GetEnd().IsDaylightSavingTime());


            _interval = new Interval() { Start = new DateTime(2017, 3, 26, 0, 0, 0, DateTimeKind.Utc), Duration = 3600 * 6 };

            Assert.IsTrue(_interval.GetEnd().Kind == DateTimeKind.Utc);
            Assert.IsTrue(_interval.GetEnd().Hour == 6);
            Assert.IsTrue(_interval.GetEnd().Minute == 0);
            Assert.IsFalse(_interval.GetEnd().IsDaylightSavingTime()); //always FALSE for UTC Time

            _interval = new Interval() { Start = new DateTime(2017, 10, 29, 0, 0, 0, DateTimeKind.Utc), Duration = 3600 * 6 };

            Assert.IsTrue(_interval.GetEnd().Kind == DateTimeKind.Utc);
            Assert.IsTrue(_interval.GetEnd().Hour == 6);
            Assert.IsTrue(_interval.GetEnd().Minute == 0);
            Assert.IsFalse(_interval.GetEnd().IsDaylightSavingTime()); //always FALSE for UTC Time
        }

        [TestMethod]
        public void TestFilterIntervalReadings()
        {
            var readings = new List<IntervalReading>();
            readings.Add(new IntervalReading { TimePeriod = new Interval { Start = DateTime.Parse("2017-10-28T01:00:01+02:00") } });
            readings.Add(new IntervalReading { TimePeriod = new Interval { Start = DateTime.Parse("2017-10-28T02:00:10+02:00") } });
            readings.Add(new IntervalReading { TimePeriod = new Interval { Start = DateTime.Parse("2017-10-28T03:10:01+02:00") } });
            readings.Add(new IntervalReading { TimePeriod = new Interval { Start = DateTime.Parse("2017-10-28T04:00:01+02:00") } });

            readings.FilterIntervalReadings(3600);
            Assert.AreEqual("2017-10-28T01:00:01+02:00", readings[0].TimePeriod.Start.ToString("yyyy-MM-ddTHH:mm:ssK"));
            Assert.AreEqual("2017-10-28T02:00:10+02:00", readings[1].TimePeriod.Start.ToString("yyyy-MM-ddTHH:mm:ssK"));
            Assert.AreEqual("2017-10-28T04:00:01+02:00", readings[2].TimePeriod.Start.ToString("yyyy-MM-ddTHH:mm:ssK"));
        }
    }
}
