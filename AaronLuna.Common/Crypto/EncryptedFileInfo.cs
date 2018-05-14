namespace AaronLuna.Common.Crypto
{
    using System.IO;

    public class EncryptedFileInfo
    {
        string _regularFilePath;
        string _encryptedFilePath;

        public FileInfo RegularFile
        {
            get => new FileInfo(_encryptedFilePath);
            set => _encryptedFilePath = value.ToString();
        }

        public FileInfo EncryptedFile
        {
            get => new FileInfo(_encryptedFilePath);
            set => _encryptedFilePath = value.ToString();
        }

    }
}
