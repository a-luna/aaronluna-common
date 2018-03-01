namespace AaronLuna.Common.Console
{
    using System;
    using System.Text;
    using System.Threading;

    public class ConsoleProgressBar : IDisposable, IProgress<double>
    {
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
            StartBracketChar = '[';
            EndBracketChar = ']';
            CompletedBlockChar = '#';
            UncompletedBlockChar = '-';
            AnimationSequence = @"|/-\-";
        }

        public int NumberOfBlocks { get; set; }
        public char StartBracketChar { get; set; }
        public char EndBracketChar { get; set; }
        public char CompletedBlockChar { get; set; }
        public char UncompletedBlockChar { get; set; }
        public string AnimationSequence { get; set; }

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
            var completedBlocks = new string(CompletedBlockChar, numBlocksCompleted);
            var uncompletedBlocks = new string(UncompletedBlockChar, NumberOfBlocks - numBlocksCompleted);

            var progressBar = $"{StartBracketChar}{completedBlocks}{uncompletedBlocks}{EndBracketChar}";
            var percent = (int)(currentProgress * 100);
            var animation = AnimationSequence[_animationIndex++ % AnimationSequence.Length];

            if (currentProgress is 1)
            {
                animation = ' ';
            }

            return $"{progressBar} {percent}% {animation}";            
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
