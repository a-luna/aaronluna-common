namespace AaronLuna.Common.Console
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    using AaronLuna.Common.Extensions;

    using Microsoft.VisualBasic;

    public static class ConsoleUtilities
    {
        const string nums =
            "0 1 2 3 4 5 6 7 8 9 _ 0 1 2 3 4 5 6 7 8 9 _ 0 1 2 3 4 5 6 7 8 9 _ 0 1 2 3 4 5 6 7 8 9 _ 0 1 2 3 4 5 6 7 8 9";

        public static void WriteCharSetToConsole(Encoding outputEncoding, int charCount)
        {
            Console.OutputEncoding = outputEncoding;
            var charIndex = 0;

            Console.WriteLine($"{Environment.NewLine}({charIndex})");
            Console.WriteLine(nums);

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
                Console.WriteLine(nums);

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
                Console.WriteLine(nums);
            }

            return charIndex;
        }

        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        const int VK_RETURN = 0x0D;
        const int WM_KEYDOWN = 0x100;

        public static void Execute()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(500);

                var hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                PostMessage(hWnd, WM_KEYDOWN, VK_RETURN, 0);
            });
        }
    }
}
