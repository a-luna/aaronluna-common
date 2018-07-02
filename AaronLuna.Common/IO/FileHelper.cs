using System.Linq;
using System.Security;

namespace AaronLuna.Common.IO
{
    using Result;
    using System;
    using System.IO;

    public static class FileHelper
    {
        public const double OneKb = 1024;
        public const double OneMb = 1024 * 1024;
        public const double OneGb = 1024 * 1024 * 1024;

        static readonly object File = new object();

        public static Result DeleteFileIfAlreadyExists(string filePath, int maxAttempts)
        {
            foreach (var attempt in Enumerable.Range(0, maxAttempts))
            {
                try
                {
                    lock (File)
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
                    if (attempt < maxAttempts) continue;
                    return Result.Fail($"{ex.Message} ({ex.GetType()} raised in method FileHelper.DeleteFileIfAlreadyExists)");
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Result.Fail($"{ex.Message} ({ex.GetType()} raised in method FileHelper.DeleteFileIfAlreadyExists)");
                }
                catch (SecurityException ex)
                {
                    return Result.Fail($"{ex.Message} ({ex.GetType()} raised in method FileHelper.DeleteFileIfAlreadyExists)");
                }
                catch (Exception ex)
                {
                    if (attempt < maxAttempts) continue;
                    return Result.Fail<int>($"{ex.Message} ({ex.GetType()} raised in method FileHelper.WriteBytesToFile)");
                }
            }

            return Result.Ok();
        }

        public static Result<int> WriteBytesToFile(string filePath, byte[] buffer, int length, int maxAttempts)
        {
            var count = 0;
            foreach (var attempt in Enumerable.Range(0, maxAttempts))
            {
                try
                {
                    lock (File)
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

                    count = attempt;
                    break;
                }
                catch (IOException ex)
                {
                    if (attempt < maxAttempts) continue;
                    return Result.Fail<int>($"{ex.Message} ({ex.GetType()} raised in method FileHelper.WriteBytesToFile)");
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Result.Fail<int>($"{ex.Message} ({ex.GetType()} raised in method FileHelper.WriteBytesToFile)");
                }
                catch (Exception ex)
                {
                    if (attempt < maxAttempts) continue;
                    return Result.Fail<int>($"{ex.Message} ({ex.GetType()} raised in method FileHelper.WriteBytesToFile)");
                }
            }

            return Result.Ok(count);
        }

        public static string FileSizeToString(long fileSizeInBytes)
        {
            if (fileSizeInBytes > OneGb)
            {
                return $"{fileSizeInBytes / OneGb:F2} GB";
            }

            if (fileSizeInBytes > OneMb)
            {
                return $"{fileSizeInBytes / OneMb:F2} MB";
            }

            return fileSizeInBytes > OneKb
                ? $"{fileSizeInBytes / OneKb:F2} KB"
                : $"{fileSizeInBytes} bytes";
        }
    }
}
