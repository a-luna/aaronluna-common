namespace AaronLuna.Common.Threading
{
    using System.Threading;

    public class InterlockedBoolSetOnce
    {
        const int FALSE = 0;
        const int TRUE = 1;
        int _state = TRUE;

        public bool CheckAndSet => Interlocked.Exchange(ref _state, FALSE) == TRUE;
    }
}
