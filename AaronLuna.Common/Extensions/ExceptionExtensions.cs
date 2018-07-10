using System;

namespace AaronLuna.Common.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetReport(this Exception ex)
        {
            var errorMessage =
                $"Exception Type: {ex.GetType()}{Environment.NewLine}" +
                $"Error Message: {Environment.NewLine}{ex.Message}";

            if (ex.InnerException != null)
            {
                errorMessage +=
                    Environment.NewLine + Environment.NewLine +
                    $"Inner Exception: {Environment.NewLine}{ex.InnerException.Message}";
            }

            errorMessage +=
                Environment.NewLine + Environment.NewLine +
                $"Stack Trace: {Environment.NewLine}{ex.StackTrace}";

            return errorMessage;
        }

        public static string GetErrorReportForLogFile(this Exception ex)
        {
            var errorMessage =
                $"\tException Type: {ex.GetType()}{Environment.NewLine}{Environment.NewLine}" +
                $"\tError Message: {Environment.NewLine}\t{ex.Message}";

            if (ex.InnerException != null)
            {
                errorMessage +=
                    Environment.NewLine + Environment.NewLine +
                    $"\tInner Exception: {Environment.NewLine}\t{ex.InnerException.Message}";
            }

            errorMessage +=
                Environment.NewLine + Environment.NewLine +
                $"\tStack Trace: {Environment.NewLine}\t{ex.StackTrace}";

            return errorMessage;
        }
    }
}
