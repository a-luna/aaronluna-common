namespace AaronLuna.Common.Console
{
    using System;
    using System.Linq;
    using System.Text;

    using Extensions;

    using Microsoft.VisualBasic;

    public static class ConsoleUtilities
    {
        const string CharIndices =
            "0 1 2 3 4 5 6 7 8 9 _ 0 1 2 3 4 5 6 7 8 9 _ 0 1 2 3 4 5 6 7 8 9 _ 0 1 2 3 4 5 6 7 8 9 _ 0 1 2 3 4 5 6 7 8 9";

        public static void WriteCharSetToConsole(Encoding outputEncoding, int charCount)
        {
            Console.OutputEncoding = outputEncoding;
            var charIndex = 0;

            Console.WriteLine($"{Environment.NewLine}({charIndex})");
            Console.WriteLine(CharIndices);

            if (charCount < 50)
            {
                PrintLessThanFiftyChars(charIndex, charCount);
                return;
            }

            charIndex = PrintMoreThanFiftyChars(charCount);
            var excessChars = charCount % 50;

            if (excessChars > 0)
            {
                Console.WriteLine($"{Environment.NewLine}({charIndex + 1})");
                Console.WriteLine(CharIndices);

                PrintLessThanFiftyChars(charIndex, excessChars);
            }

            Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}Specified range of characters successfully output to screen.{Environment.NewLine}Press any key to continue.");
            Console.ReadKey();
        }

        static void PrintLessThanFiftyChars(int startIndex, int totalChars)
        {
            foreach (var i in Enumerable.Range(0, totalChars))
            {
                var charIndex = startIndex + i;
                var thisChar = Strings.ChrW(charIndex);
                Console.Write($"{thisChar} ");

                if ((i + 1) % 10 == 0)
                {
                    Console.Write("  ");
                }
            }
        }

        static Int32 PrintMoreThanFiftyChars(int totalChars)
        {
            var totalLoops = totalChars / 50;
            var charIndex = 0;

            foreach (var i in Enumerable.Range(0, totalLoops))
            {
                foreach (var j in Enumerable.Range(0, 5))
                {
                    foreach (var k in Enumerable.Range(0, 10))
                    {
                        charIndex = (i * 50) + (j * 10) + k;
                        if (charIndex < 32)
                        {
                            Console.Write("  ");
                            continue;
                        }
                        var thisChar = Strings.ChrW(charIndex);
                        Console.Write($"{thisChar} ");
                    }
                    Console.Write("  ");
                }
                // break every 50 chars
                Console.WriteLine();
                if (i.IsLastIteration(totalLoops)) break;

                Console.WriteLine($"{Environment.NewLine}({charIndex + 1})");
                Console.WriteLine(CharIndices);
            }

            return charIndex;
        }
    }
}
