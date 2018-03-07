using System.Linq;
using AaronLuna.Common.Numeric;

namespace AaronLuna.Common.Console
{
    using System;
    using System.Text;
    using System.Threading;

    public class ConsoleProgressBar : IDisposable, IProgress<double>
    {
        public static string RotatingArrowAnimation = "\u2190\u2196\u2191\u2197\u2192\u2198\u2193\u2199";
        public static string GrowingBarAnimation = "\u2581\u2582\u2583\u2584\u2585\u2586\u2587\u2588\u2587\u2586\u2585\u2584\u2583\u2581";
        public static string BrailleAnimation1 = "\u28fe\u28fd\u28fb\u28bf\u287f\u28df\u28ef\u28f7";
        public static string BrailleAnimation2 = "\u2801\u2802\u2804\u2840\u2880\u2820\u2810\u2808";
        public static string EyeAnimation = "\u25e1\u25e1\u2299\u2299\u25e0\u25e0";
        public static string SemicircleAnimation = "\u25d0\u25d3\u25d1\u25d2";
        public static string RotatingTriangleAnimation = "\u25e2\u25e3\u25e4\u25e5";
        public static string RotatingSquareAnimation = "\u2596\u2598\u259d\u2597";
        public static string RotatingPipeAnimation = "\u2524\u2518\u2534\u2514\u251c\u250c\u252c\u2510";
        public static string BouncingBallAnimation = ".oO\u00b0Oo.";
        public static string ExplodingAnimation = ".oO@*";
        public static string DefaultSpinnerAnimation = @"|/-\-";

        readonly TimeSpan _animationInterval = TimeSpan.FromSeconds(1.0 / 8);

        Timer _timer;
        double _currentProgress;
        string _currentText = string.Empty;
        bool _disposed;
        int _animationIndex;

        public ConsoleProgressBar()
        {
            Initialize();

            NumberOfBlocks = 10;
            StartBracket = "[";
            EndBracket = "]";
            CompletedBlock = "#";
            UncompletedBlock = "-";
            AnimationSequence = DefaultSpinnerAnimation;
        }

        public int NumberOfBlocks { get; set; }
        public string StartBracket { get; set; }
        public string EndBracket { get; set; }
        public string CompletedBlock { get; set; }
        public string UncompletedBlock { get; set; }
        public string AnimationSequence { get; set; }        
        public long BytesReceived { get; set; }
        public long FileSizeInBytes { get; set; }

        public void Report(double value)
        {
            // Make sure value is in [0..1] range
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref _currentProgress, value);
        }

        private void Initialize()
        {
            _timer = new Timer(TimerHandler);

            // A progress bar is only for temporary display in a console window.
            // If the console output is redirected to a file, draw nothing.
            // Otherwise, we'll end up with a lot of garbage in the target file.
            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        private void TimerHandler(object state)
        {
            lock (_timer)
            {
                if (_disposed) return;

                var currentProgress = GetProgressBarText(_currentProgress);
                UpdateText(currentProgress);
                ResetTimer();
            }
        }

        private string GetProgressBarText(double currentProgress)
        {
            var numBlocksCompleted = (int)(currentProgress * NumberOfBlocks);

            var completedBlocks = string.Empty;
            foreach (var i in Enumerable.Range(0, numBlocksCompleted))
            {
                completedBlocks += CompletedBlock;
            }

            var uncompletedBlocks = string.Empty;
            foreach (var i in Enumerable.Range(0, NumberOfBlocks - numBlocksCompleted))
            {
                uncompletedBlocks += UncompletedBlock;
            }            

            var progressBar = $"{StartBracket}{completedBlocks}{uncompletedBlocks}{EndBracket}";
            var percent = (int)(currentProgress * 100);
            var bytesReceived = BytesReceived.ConvertBytesForDisplay();
            var fileSizeInBytes = FileSizeInBytes.ConvertBytesForDisplay();
            var animation = AnimationSequence[_animationIndex++ % AnimationSequence.Length];

            if (currentProgress is 1)
            {
                animation = ' ';
            }

            return $"{progressBar} {percent}% {bytesReceived} of {fileSizeInBytes} {animation}";
        }

        private void UpdateText(string text)
        {
            // Get length of common portion
            var commonPrefixLength = 0;
            var commonLength = Math.Min(_currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == _currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            // Backtrack to the first differing character
            var outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', _currentText.Length - commonPrefixLength);

            // Output new suffix
            outputBuilder.Append(text.Substring(commonPrefixLength));

            // If the new text is shorter than the old one: delete overlapping characters
            var overlapCount = _currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            //Console.Write($"{Caption}{outputBuilder}");
            Console.Write(outputBuilder);
            _currentText = text;
        }

        private void ResetTimer()
        {
            _timer.Change(_animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (_timer)
            {
                _disposed = true;
                UpdateText(GetProgressBarText(1));
            }
        }
    }
}
