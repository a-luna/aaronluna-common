namespace AaronLuna.Common.Numeric
{
    public static class NumericExtensions
    {
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
