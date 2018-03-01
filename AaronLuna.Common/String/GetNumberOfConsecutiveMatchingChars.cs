namespace AaronLuna.Common.String
{
    public static class StringFunctions
    {
        public static int GetNumberOfConsecutiveMatchingChars(string a, string b, bool handleLengthDifference)
        {
            var equalsReturnCode = -1;
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            {
                return handleLengthDifference ? 0 : equalsReturnCode;
            }

            var longest = b.Length > a.Length ? b : a;
            var shorten = b.Length > a.Length ? a : b;
            for (var i = 0; i < shorten.Length; i++)
            {
                if (shorten[i] != longest[i])
                {
                    return i;
                }
            }

            // Handles cases when length is different (a="1234", b="123")
            // index=3 would be returned for this case
            // If you do not need such behaviour - just remove this
            if (handleLengthDifference && a.Length != b.Length)
            {
                return shorten.Length;
            }

            return equalsReturnCode;
        }
    }
}
