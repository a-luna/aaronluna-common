using System;

namespace AaronLuna.Common.Console
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs()
        {
            LastDataReceived = DateTime.MinValue;
            TimerIntervalTriggered = DateTime.MinValue;
        }

        public DateTime LastDataReceived { get; set; }
        public DateTime TimerIntervalTriggered { get; set; }
        public TimeSpan Elapsed => TimerIntervalTriggered - LastDataReceived;
    }
}
