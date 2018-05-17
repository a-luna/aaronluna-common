using System;
using System.Collections.Generic;
using System.IO;
using AaronLuna.Common.IO;

namespace AaronLuna.CommonTest.IO
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ZipperTestFixture
    {

        [TestMethod]
        public void VerifyCreateArchive()
        {
            var currentPath = Directory.GetCurrentDirectory();
            var index = currentPath.IndexOf("bin", StringComparison.Ordinal);
            var testFilesFolder = $"{currentPath.Remove(index - 1)}{Path.DirectorySeparatorChar}IO{Path.DirectorySeparatorChar}ZipperTestFixture{Path.DirectorySeparatorChar}";
            var outputFile = Path.Combine(testFilesFolder, "test.zip");

            var filesToCompress = new List<FileInfo>
            {
                new FileInfo(Path.Combine(testFilesFolder, "file1.txt")),
                new FileInfo(Path.Combine(testFilesFolder, "file2.txt")),
                new FileInfo(Path.Combine(testFilesFolder, "file3.txt")),
                new FileInfo(Path.Combine(testFilesFolder, "file4.txt")),
                new FileInfo(Path.Combine(testFilesFolder, "file5.txt"))
            };

            var result = Zipper.CreateArchive(filesToCompress, outputFile);
            if (result.Failure)
            {
                Assert.Fail(result.Error);
            }

            Assert.IsTrue(File.Exists(outputFile));
            File.Delete(outputFile);
        }
    }
}
