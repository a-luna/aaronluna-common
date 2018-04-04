namespace AaronLuna.Common.IO
{
    using Result;
    using System;
    using System.IO;

    public static class FileHelper
    {
        public const double OneKB = 1024;
        public const double OneMB = 1024 * 1024;
        public const double OneGB = 1024 * 1024 * 1024;

        public static Result DeleteFileIfAlreadyExists(string filePath)
        {
            try
            {
                var fi = new FileInfo(filePath);
                if (!fi.Exists)
                {
                    return Result.Ok();
                }

                fi.Delete();
            }
            catch (IOException ex)
            {
                return Result.Fail($"{ex.Message} ({ex.GetType()} raised in method FileHelper.DeleteFileIfAlreadyExists)");
            }

            return Result.Ok();
        }

        public static Result WriteBytesToFile(string filePath, byte[] buffer, int length)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Append))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(buffer, 0, length);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Result.Fail($"{ex.Message} ({ex.GetType()} raised in method FileHelper.DeleteFileIfAlreadyExists)");
            }

            return Result.Ok();
        }

        public static string GetTransferRate(TimeSpan elapsed, long bytesReceived)
        {
            if (elapsed == TimeSpan.MinValue || bytesReceived == 0)
            {
                return string.Empty;
            }

            var elapsedMilliseconds = elapsed.Ticks / (double) 10_000;
            var bytesPerSecond = (bytesReceived * 1000) / elapsedMilliseconds;
            var kilobytesPerSecond = bytesPerSecond / 1024;
            var megabytesPerSecond = kilobytesPerSecond / 1024;

            if (megabytesPerSecond > 1)
            {
                return $"{megabytesPerSecond:F1} MB/s";
            }

            return kilobytesPerSecond > 1
                ? $"{kilobytesPerSecond:F1} KB/s"
                : $"{bytesPerSecond:F1} bytes/s";
        }

        public static string FileSizeToString(long fileSizeInBytes)
        {
            if (fileSizeInBytes > OneGB)
            {
                return $"{fileSizeInBytes / OneGB:F2} GB";
            }

            if (fileSizeInBytes > OneMB)
            {
                return $"{fileSizeInBytes / OneMB:F2} MB";
            }

            return fileSizeInBytes > OneKB
                ? $"{fileSizeInBytes / OneKB:F2} KB"
                : $"{fileSizeInBytes} bytes";
        }
    }
}
