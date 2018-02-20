using System;

namespace AaronLuna.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        public static bool IsWithinCurrentWeek(this DateTime dt)
        {
            var startOfWeek = DateTime.Today.StartOfWeek(DayOfWeek.Monday);
            var endOfWeek = startOfWeek.AddDays(7);

            return dt >= startOfWeek && dt < endOfWeek;
        }

        public static bool IsWithinCurrentMonth(this DateTime dt)
        {
            var thisYearNum = DateTime.Now.Year;
            var thisMonthNum = DateTime.Now.Month;

            return thisYearNum == dt.Year && thisMonthNum == dt.Month;
        }
    }
}
