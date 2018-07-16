namespace AaronLuna.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static List<string> Clone(this List<string> list)
        {
            return list.Select(str => (string)str.Clone()).ToList();
        }

        public static bool IsEqualTo<T>(this List<T> thisList, List<T> otherList)
        {
            thisList.Sort();
            otherList.Sort();

            return thisList.SequenceEqual(otherList);
        }

        public static string ConvertListToString<T>(this List<T> list, string separator)
        {
            var listStr = String.Empty;
            foreach (var i in Enumerable.Range(0, list.Count))
            {
                listStr += list[i].ToString();

                if (i < list.Count - 1)
                {
                    listStr += separator;
                }
            }

            return listStr;
        }

        public static string FormatAsCsv(this List<string> list)
        {
            return ConvertListToString(list, ",");
        }

        public static DateTime ToDateTimeFromFormat1(this string input)
        {
            if (input.Length != 8) return DateTime.MinValue;

            var yearString = input.Substring(0, 4);
            var monthString = input.Substring(4, 2);
            var dayString = input.Substring(6, 2);

            return ConvertThreeStringValuesToDateTime(yearString, monthString, dayString);
        }

        public static DateTime ToDateTimeFromFormat2(this string input)
        {
            var split = input.Split('/').ToList();
            if (split.Count != 3) return DateTime.MinValue;

            var monthString = split[0];
            var dayString = split[1];
            var yearString = split[2];

            return ConvertThreeStringValuesToDateTime(yearString, monthString, dayString);
        }

        public static string ToMacAddress(this string input)
        {
            if (input.Length != 12) return String.Empty;

            var oct1 = input.Substring(0, 2);
            var oct2 = input.Substring(2, 2);
            var oct3 = input.Substring(4, 2);
            var oct4 = input.Substring(6, 2);
            var oct5 = input.Substring(8, 2);
            var oct6 = input.Substring(10, 2);

            return $"{oct1}:{oct2}:{oct3}:{oct4}:{oct5}:{oct6}";
        }

        public static bool HexStringRepresentsNegativeNumber(this string hexString)
        {
            if (string.IsNullOrEmpty(hexString) || !hexString.ContainsOnlyHexadecimalDigits())
            {
                return false;
            }

            var bytes = StringToByteArray(hexString).ToList();
            if (bytes.Count <= 0) return false;

            var mostSignificantByte = Convert.ToString(bytes[0], 2);
            return mostSignificantByte.StartsWith("1", StringComparison.InvariantCulture);
        }

        public static bool HexStringRepresentsPositiveNumber(this string hexString)
        {
            if (string.IsNullOrEmpty(hexString) || !hexString.ContainsOnlyHexadecimalDigits())
            {
                return false;
            }

            var bytes = StringToByteArray(hexString).ToList();
            if (bytes.Count <= 0) return false;

            var mostSignificantByte = Convert.ToString(bytes[0], 2);
            return mostSignificantByte.StartsWith("0", StringComparison.InvariantCulture);
        }

        static byte[] StringToByteArray(string hexString)
        {
            var sanitizedHex = hexString;
            if (hexString.StartsWith("0x", StringComparison.InvariantCulture))
            {
                sanitizedHex = hexString.Substring(2);
            }

            sanitizedHex = sanitizedHex.PadLeft(16, '0');
            var numberChars = sanitizedHex.Length;
            var bytes = new byte[numberChars / 2];

            for (var i = 0; i < numberChars; i += 2)
            {
                var currentByte = sanitizedHex.Substring(i, 2);
                bytes[i / 2] = Convert.ToByte(currentByte, 16);
            }

            return bytes;
        }

        public static bool ContainsOnlyHexadecimalDigits(this string input)
        {
            var hexChars = Constants.HexadecimalDigits.ToCharArray().ToList();
            var inputChars = input.Distinct().ToList();
            var invalidChars = inputChars.Select(ch => ch).Where(ch => !hexChars.Contains(ch)).ToList();

            return invalidChars.Count == 0;
        }

        public static bool ContainsOnlyAlphabeticCharacters(this string input)
        {
            var alphabeticCharacters = Constants.AlphabeticCharacterSet.ToCharArray().ToList();
            var inputCharacters = input.Distinct().ToList();
            var invalidCharacters =
                inputCharacters.Select(ch => ch).Where(ch => !alphabeticCharacters.Contains(ch)).ToList();

            return invalidCharacters.Count == 0;
        }

        public static bool ContainsOnlyAlphanumericCharacters(this string input)
        {
            var alphanumericCharacters = Constants.AlphanumericCharacterSet.ToCharArray().ToList();
            var inputCharacters = input.Distinct().ToList();
            var invalidCharacters =
                inputCharacters.Select(ch => ch).Where(ch => !alphanumericCharacters.Contains(ch)).ToList();

            return invalidCharacters.Count == 0;
        }

        public static bool IsValidFileName(this string fileName)
        {
            var allowedChars = Constants.CharsAllowedInFileNames.ToCharArray().ToList();
            var inputCharacters = fileName.Distinct().ToList();
            var invalidCharacters =
                inputCharacters.Select(ch => ch).Where(ch => !allowedChars.Contains(ch)).ToList();

            return invalidCharacters.Count == 0;
        }

        public static List<string> SplitStringOnChar(this string s, char splitter)
        {
            var split = s.Split(splitter);
            return Enumerable.Range(0, split.Length).Select(i => split[i]).ToList();
        }

        static DateTime ConvertThreeStringValuesToDateTime(string yearString, string monthString, string dayString)
        {
            var didParseYear = int.TryParse(yearString, out var parsedYear);
            var didParseMonth = int.TryParse(monthString, out var parsedMonth);
            var didParseDay = int.TryParse(dayString, out var parsedDay);

            if (!didParseYear || !didParseMonth || !didParseDay) return DateTime.MinValue;

            var yearIsValid = parsedYear >= 1 && parsedYear <= 9999;
            var monthIsValid = parsedMonth >= 1 && parsedMonth <= 12;
            var dayIsValid = parsedDay >= 1 && parsedDay <= 31;

            if (!yearIsValid || !monthIsValid || !dayIsValid) return DateTime.MinValue;

            var parsedDateTime = DateTime.MinValue;
            try
            {
                parsedDateTime = new DateTime(parsedYear, parsedMonth, parsedDay);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    return DateTime.MinValue;
                }
            }

            return parsedDateTime;
        }
    }
}
