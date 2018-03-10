namespace AaronLuna.Common.IO
{
    using Result;
    using System;
    using System.IO;

    public static class FileHelper
    {
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
                    bw.Close();
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

            var totalMilliseconds = elapsed.Ticks / 10_000;
            var bytesPerMs = bytesReceived / (double)totalMilliseconds;
            var bytesPerSecond = bytesPerMs * 1000;
            var kilobytesPerSecond = bytesPerSecond / 1024;
            var megabytesPerSecond = kilobytesPerSecond / 1024;

            if (megabytesPerSecond > 1)
            {
                return $"{megabytesPerSecond:F1} MB/s";
            }

            if (kilobytesPerSecond > 1)
            {
                return $"{kilobytesPerSecond:F1} KB/s";
            }

            return $"{bytesPerSecond:F1} bytes/s";
        }

        public static string FileSizeToString(long fileSizeInBytes)
        {
            const float oneKb = 1024;
            const float oneMb = 1024 * 1024;
            const float oneGb = 1024 * 1024 * 1024;

            if (fileSizeInBytes > oneGb)
            {
                return $"{fileSizeInBytes / oneGb:#.##} GB";
            }

            if (fileSizeInBytes > oneMb)
            {
                return $"{fileSizeInBytes / oneMb:#.##} MB";
            }

            if (fileSizeInBytes > oneKb)
            {
                return $"{fileSizeInBytes / oneKb:#.##} KB";
            }

            return $"{fileSizeInBytes} fileSizeInBytes";
        }
    }
}
