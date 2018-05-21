namespace AaronLuna.Common.Logging
{
    using System;

    public static class Logging
    {
        public const string DEBUG = "DEBUG";
        public const string INFO = "INFO";
        public const string WARN = "WARN";
        public const string ERROR = "ERROR";

        public static string GetTimeStampForFileName(DateTime dt)
        {
            var yearNum = dt.Year;
            var monthNum = dt.Month;
            var dayNum = dt.Day;
            var hourNum = dt.Hour;
            var minuteNum = dt.Minute;
            var secondNum = dt.Second;

            var year = yearNum.ToString();
            var month = monthNum.ToString();
            var day = dayNum.ToString();
            var hour = hourNum.ToString();
            var minute = minuteNum.ToString();
            var second = secondNum.ToString();

            if (yearNum < 10)
            {
                year = $"0{yearNum}";
            }

            if (monthNum < 10)
            {
                month = $"0{monthNum}";
            }

            if (dayNum < 10)
            {
                day = $"0{dayNum}";
            }

            if (hourNum < 10)
            {
                hour = $"0{hourNum}";
            }

            if (minuteNum < 10)
            {
                minute = $"0{minuteNum}";
            }

            if (secondNum < 10)
            {
                second = $"0{secondNum}";
            }

            return $"{year}{month}{day}_{hour}{minute}{second}";
        }

        public static string GetTimeStampForFileName()
        {
            return GetTimeStampForFileName(DateTime.Now);
        }
    }
}
