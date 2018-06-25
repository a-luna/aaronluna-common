using System.Collections.Generic;
using AaronLuna.Common.Extensions;

namespace AaronLuna.Common.Numeric
{
    using System;
    using System.Linq;

    public static class ByteConverter
    {
        public static string ToHexString(this byte[] bytes, int bytesPerLine = 0)
        {
            var hexList = ConvertBytesToListOfHexStrings(bytes);
            if (bytesPerLine == 0 || hexList.Count <= bytesPerLine)
            {
                return hexList.ConvertListToString(" ");
            }

            return FormatListofHexStringsForDisplay(hexList, bytesPerLine);
        }

        static List<string> ConvertBytesToListOfHexStrings(IEnumerable<byte> bytes)
        {
            var byteList = bytes.ToList();
            byteList.Reverse();

            var byteStrings = new List<string>();
            foreach (var b in byteList)
            {
                if (int.TryParse(b.ToString(), out var h))
                {
                    byteStrings.Add($"{h:X2}");
                }
            }

            return byteStrings;
        }

        static string FormatListofHexStringsForDisplay(IReadOnlyList<string> hexList, int bytesPerLine)
        {
            var hex = string.Empty;
            var lineCount = hexList.Count / bytesPerLine;
            var remainderCount = hexList.Count % bytesPerLine;

            foreach (var i in Enumerable.Range(0, lineCount))
            {
                var startIndex = i * bytesPerLine;
                foreach (var j in Enumerable.Range(0, bytesPerLine))
                {
                    hex += hexList[startIndex + j];

                    if (j.IsLastIteration(bytesPerLine))
                    {
                        hex += Environment.NewLine;
                    }
                    else
                    {
                        hex += " ";
                    }
                }
            }

            hex =
                Enumerable.Range(0, remainderCount).Aggregate(
                    hex,
                    (current, i) => current + $"{hexList[lineCount * bytesPerLine + i]} ");

            return hex.Trim();
        }
    }
}
