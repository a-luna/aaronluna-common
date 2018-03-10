namespace AaronLuna.Common.Numeric
{
    public static class NumericExtensions
    {
        public static string ConvertBytesForDisplay(this long bytes)
        {
            const float oneKb = 1024;
            const float oneMb = 1024 * 1024;
            const float oneGb = 1024 * 1024 * 1024;

            if (bytes > oneGb)
            {
                return $"{bytes / oneGb:#.##} GB";
            }

            if (bytes > oneMb)
            {
                return $"{bytes / oneMb:#.##} MB";
            }

            if (bytes > oneKb)
            {
                return $"{bytes / oneKb:#.##} KB";
            }

            return $"{bytes} bytes";
        }

        public static bool IsLastIteration(this int i, int count)
        {
            return i == count - 1;
        }

        public static bool IsEven(this int num)
        {
            return (num & 1) == 0;
        }

        public static bool IsOdd(this int num)
        {
            return (num & 1) != 0;
        }
    }
}
