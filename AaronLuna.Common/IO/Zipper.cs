namespace AaronLuna.Common.IO
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    using Result;

    public static class Zipper
    {
        public static Result CreateArchive(List<FileInfo> filesToCompress, string outputFile, bool deleteIfAlreadyExists=false)
        {
            var outputFolder = Path.GetDirectoryName(outputFile);
            Directory.CreateDirectory(outputFolder);

            if (File.Exists(outputFile))
            {
                if (!deleteIfAlreadyExists)
                {
                    return Result.Fail($"A file named \"{Path.GetFileName(outputFile)}\" already exists at {outputFolder}.");
                }

                File.Delete(outputFile);
            }

            if (filesToCompress.Count <= 0)
            {
                return Result.Fail("File list contains zero files.");
            }

            try
            {
                using (var fs = new FileStream(outputFile, FileMode.Create))
                using (var arch = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    foreach (var file in filesToCompress)
                    {
                        arch.CreateEntryFromFile(file.FullName, file.Name);
                    }
                }
            }
            catch (IOException ex)
            {
                return Result.Fail($"{ex.Message} {ex.GetType()}");
            }

            return Result.Ok();
        }
    }
}
