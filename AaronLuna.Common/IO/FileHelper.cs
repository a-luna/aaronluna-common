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

        static object _file = new object();

        public static Result DeleteFileIfAlreadyExists(string filePath)
        {
            try
            {
                lock (_file)
                {
                    var fi = new FileInfo(filePath);
                    if (!fi.Exists)
                    {
                        return Result.Ok();
                    }

                    fi.Delete();
                }
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
                lock (_file)
                {
                    using (var fs = new FileStream(
                        filePath,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.None))
                    using (var bw = new BinaryWriter(fs))
                    {
                        bw.Write(buffer, 0, length);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Result.Fail($"{ex.Message} ({ex.GetType()} raised in method FileHelper.WriteBytesToFile)");
            }
            catch (IOException ex)
            {
                return Result.Fail($"{ex.Message} ({ex.GetType()} raised in method FileHelper.WriteBytesToFile)");
            }

            return Result.Ok();
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
