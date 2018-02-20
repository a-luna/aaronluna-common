namespace AaronLuna.Common.Numeric
{
    using System.IO;

    public static class NumericExtensions
    {
        public static string ConvertBytesForDisplay(this long bytes)
        {
            const float OneKb = 1024;
            const float OneMb = 1024 * 1024;
            const float OneGb = 1024 * 1024 * 1024;
            string convertedBytes;

            if (bytes > OneGb)
            {
                convertedBytes = $"{bytes / OneGb:#.##} GB";
            }
            else if (bytes > OneMb)
            {
                convertedBytes = $"{bytes / OneMb:#.##} MB";
            }
            else if (bytes > OneKb)
            {
                convertedBytes = $"{bytes / OneKb:#.##} KB";
            }
            else
            {
                convertedBytes = $"{bytes} bytes";
            }

            return convertedBytes;
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
