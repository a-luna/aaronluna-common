namespace AaronLuna.CommonTest.Extensions
{
    using System;

    using Common.Extensions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TimeSpanExtensionsTestFixture
    {
        [TestMethod]
        public void VerifyZeroTimeSpan()
        {
            var timeSpan = TimeSpan.Zero.ToFormattedString();
            Assert.AreEqual("0ms", timeSpan);
        }

        [TestMethod]
        public void VerifyNegativeTimeSpan()
        {
            var dateTime1 = new DateTime(1997, 8, 15);
            var dateTime2 = new DateTime(1996, 8, 15);
            var timeSpan = (dateTime2 - dateTime1).ToFormattedString();

            Assert.AreEqual("0ms", timeSpan);
        }

        [TestMethod]
        public void VerifyGreaterThanOneYear()
        {
            var dateTime1 = new DateTime(2012, 12, 21);
            var dateTime2 = new DateTime(2017, 12, 31);
            var timeSpan = (dateTime2 - dateTime1).ToFormattedString();

            Assert.AreEqual("5y 1w 4d", timeSpan);
        }

        [TestMethod]
        public void VerifyExactly1Year()
        {
            var dateTime1 = new DateTime(1996, 8, 15);
            var dateTime2 = new DateTime(1997, 8, 15);
            var timeSpan = (dateTime2 - dateTime1).ToFormattedString();

            Assert.AreEqual("1y", timeSpan);
        }

        [TestMethod]
        public void VerifyLeapYear()
        {
            var dateTime1 = new DateTime(2016, 1, 15);
            var dateTime2 = new DateTime(2017, 1, 15);
            var timeSpan = (dateTime2 - dateTime1).ToFormattedString();

            Assert.AreEqual("1y 1d", timeSpan);
        }

        [TestMethod]
        public void VerifyGreaterThanOneWeek()
        {
            var dateTime1 = new DateTime(1942, 12, 7);
            var dateTime2 = new DateTime(1943, 1, 17);
            var timeSpan = (dateTime2 - dateTime1).ToFormattedString();

            Assert.AreEqual("5w 6d", timeSpan);
        }

        [TestMethod]
        public void VerifyMultipleDaysWithHoursMinutesSeconds()
        {
            var dateTime1 = new DateTime(1952, 4, 23, 8, 22, 17);
            var dateTime2 = new DateTime(1952, 4, 26, 17, 30, 21);
            var timeSpan = (dateTime2 - dateTime1).ToFormattedString();

            Assert.AreEqual("3d 9h 8m 4s", timeSpan);
        }

        [TestMethod]
        public void VerifyLessThanOneMinute()
        {
            var dateTime1 = new DateTime(1931, 10, 10, 10, 10, 10);
            var dateTime2 = new DateTime(1931, 10, 10, 10, 10, 19);
            var dateTime3 = new DateTime(dateTime2.Ticks + 2_040_000);
            var timeSpan = (dateTime3 - dateTime1).ToFormattedString();

            Assert.AreEqual("9s 204ms", timeSpan);
        }

        [TestMethod]
        public void TestWithCurrentTime()
        {
            var dateTime1 = DateTime.Now;
            var dateTime2 = new DateTime(dateTime1.Ticks + 5_000_000);
            var timeSpan = (dateTime2 - dateTime1).ToFormattedString();

            Assert.AreEqual("500ms", timeSpan);
        }
    }
}
