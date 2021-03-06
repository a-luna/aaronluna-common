﻿namespace AaronLuna.Common.Threading
{
    using System.Threading;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static async Task<TResult> HandleCancellation<TResult>(
            this Task<TResult> asyncTask,
            CancellationToken cancellationToken)
        {
            // Create another task that completes as soon as cancellation is requested.
            // http://stackoverflow.com/a/18672893/1149773
            var tcs = new TaskCompletionSource<TResult>();
            cancellationToken.Register(() =>
                tcs.TrySetCanceled(), useSynchronizationContext: false);
            var cancellationTask = tcs.Task;

            // Create a task that completes when either the async operation completes,
            // or cancellation is requested.
            var readyTask = await Task.WhenAny(asyncTask, cancellationTask).ConfigureAwait(false);

            // In case of cancellation, register a continuation to observe any unhandled
            // exceptions from the asynchronous operation (once it completes).
            // In .NET 4.0, unobserved task exceptions would terminate the process.
            if (readyTask == cancellationTask)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                asyncTask.ContinueWith(_ => asyncTask.Exception,
                   TaskContinuationOptions.OnlyOnFaulted
                   | TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            return await readyTask;
        }
    }
}
