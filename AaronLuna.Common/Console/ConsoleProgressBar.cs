﻿namespace AaronLuna.Common.Console
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public class ConsoleProgressBar : IDisposable, IProgress<double>
    {
        readonly TimeSpan _animationInterval = TimeSpan.FromSeconds(1.0 / 8);

        internal Timer Timer;
        internal double CurrentProgress;
        internal bool Disposed;
        internal int AnimationIndex;

        string _currentText = string.Empty;

        public ConsoleProgressBar()
        {
            NumberOfBlocks = 10;
            StartBracket = "[";
            EndBracket = "]";
            CompletedBlock = "#";
            IncompleteBlock = "-";
            AnimationSequence = ProgressAnimations.Default;

            DisplayBar = true;
            DisplayPercentComplete = true;
            DisplayAnimation = true;

            Timer = new Timer(TimerHandler);

            // A progress bar is only for temporary display in a console window.
            // If the console output is redirected to a file, draw nothing.
            // Otherwise, we'll end up with a lot of garbage in the target file.
            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        public int NumberOfBlocks { get; set; }
        public string StartBracket { get; set; }
        public string EndBracket { get; set; }
        public string CompletedBlock { get; set; }
        public string IncompleteBlock { get; set; }
        public string AnimationSequence { get; set; }
        public bool DisplayBar { get; set; }
        public bool DisplayPercentComplete { get; set; }
        public bool DisplayAnimation { get; set; }

        public void Report(double value)
        {
            // Make sure value is in [0..1] range
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref CurrentProgress, value);
        }

        void TimerHandler(object state)
        {
            lock (Timer)
            {
                if (Disposed) return;
                UpdateText(GetProgressBarText(CurrentProgress));
                ResetTimer();
            }
        }

        string GetProgressBarText(double currentProgress)
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
                uncompletedBlocks += IncompleteBlock;
            }

            var progressBar = $"{StartBracket}{completedBlocks}{uncompletedBlocks}{EndBracket} ";
            var percent = $" {(int)(currentProgress * 100)}% ";
            var whiteSpace = " ";
            var animation = AnimationSequence[AnimationIndex++ % AnimationSequence.Length];

            if (!DisplayBar) progressBar = string.Empty;
            if (!DisplayPercentComplete) percent = string.Empty;
            if (!DisplayAnimation) animation = ' ';

            if (currentProgress is 1)
            {
                animation = ' ';
            }

            var fullBar = $"{progressBar}{percent}{whiteSpace}{animation}{whiteSpace}";
            fullBar = fullBar.Replace("  ", " ");
            fullBar.TrimEnd();

            return fullBar;
        }

        internal void UpdateText(string text)
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

        internal void ResetTimer()
        {
            Timer.Change(_animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            lock (Timer)
            {
                Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
