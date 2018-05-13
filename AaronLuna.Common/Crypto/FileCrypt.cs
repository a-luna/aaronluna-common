namespace AaronLuna.Common.Crypto
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using static System.Convert;

    using Result;

    public static class FileCrypt
    {
        public static async Task<Result> EncryptFile(string filePath, string publicRsaKeyXmlFilePath)
        {
            try
            {
                await Task.Factory.StartNew(() => Encrypt(filePath, publicRsaKeyXmlFilePath));
            }
            catch (Exception ex)
            {
                return Result.Fail($"{ex.Message} {ex.GetType()}");
            }

            return Result.Ok();
        }

        public static async Task<Result> DecryptFile(string infoXmlFilePath, string privateRsaKeyXmlFilePath)
        {
            try
            {
                await Task.Factory.StartNew(() => Decrypt(infoXmlFilePath, privateRsaKeyXmlFilePath));
            }
            catch (Exception ex)
            {
                return Result.Fail($"{ex.Message} {ex.GetType()}");
            }

            return Result.Ok();
        }

        static void Encrypt(string filePath, string publicRsaKeyXmlFilePath)
        {
            var folderPath = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);
            var encryptedFileName = $"{fileName}.encrypted";
            var encryptedFilePath = Path.Combine(folderPath, encryptedFileName);
            var infoXmlFilePath = Path.Combine(folderPath, $"{fileName}.info.xml");

            var signatureKey = GetRandomBytes(64);
            var encryptionKey = GetRandomBytes(16);
            var encryptionIV = GetRandomBytes(16);

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = 128;
                aes.Key = encryptionKey;
                aes.IV = encryptionIV;
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var fsInput = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var fsEncrypted = File.Open(encryptedFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var cs = new CryptoStream(fsEncrypted, encryptor, CryptoStreamMode.Write))
                {
                    fsInput.CopyTo(cs);
                }
            }

            var encryptedFileSignature = CalculateSha256(encryptedFilePath, signatureKey);
            var publicRsaKey = ReadRsaKeyXmlFromFile(publicRsaKeyXmlFilePath);

            CreateInfoXml(
                fileName,
                encryptedFileName,
                encryptedFileSignature,
                signatureKey,
                encryptionKey,
                encryptionIV,
                publicRsaKey,
                infoXmlFilePath);
        }

        static string ReadRsaKeyXmlFromFile(string rsaXmlFilePath)
        {
            string rsaKey;
            using (var sr = File.OpenText(rsaXmlFilePath))
            {
                rsaKey = sr.ReadToEnd();
            }

            return rsaKey;
        }

        static byte[] GetRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
            {
                random.GetBytes(bytes);
            }

            return bytes;
        }

        static byte[] CalculateSha256(string filePath, byte[] key)
        {
            byte[] sha256;
            using (var sha = new HMACSHA256(key))
            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                sha256 = sha.ComputeHash(fs);
            }

            return sha256;
        }

        static void CreateInfoXml(
            string fileName,
            string encryptedFileName,
            byte[] signature,
            byte[] signatureKey,
            byte[] encryptionKey,
            byte[] encryptionIv,
            string rsaKey,
            string manifestFilePath)
        {
            var template = "<DataInfo>" +
                              $"<FileName>{fileName}</FileName>" +
                              $"<EncryptedFileName>{encryptedFileName}</EncryptedFileName>" +
                              "<Encrypted>True</Encrypted>" +
                              "<KeyEncryption algorithm='RSA2048'>" +
                              "</KeyEncryption>" +
                              "<DataEncryption algorithm='AES128'>" +
                              "<AESEncryptedKeyValue>" +
                              "<Key/>" +
                              "<IV/>" +
                              "</AESEncryptedKeyValue>" +
                              "</DataEncryption>" +
                              "<DataSignature algorithm='HMACSHA256'>" +
                              "<Value />" +
                              "<EncryptedKey />" +
                              "</DataSignature>" +
                              "</DataInfo>";

            var doc = XDocument.Parse(template);

            doc.Descendants("DataEncryption")
                .Single().Descendants("AESEncryptedKeyValue")
                .Single().Descendants("Key")
                .Single().Value = ToBase64String(EncryptBytesRsa(encryptionKey, rsaKey));

            doc.Descendants("DataEncryption")
                .Single().Descendants("AESEncryptedKeyValue")
                .Single().Descendants("IV")
                .Single().Value = ToBase64String(EncryptBytesRsa(encryptionIv, rsaKey));

            doc.Descendants("DataSignature")
                .Single().Descendants("Value")
                .Single().Value = ToBase64String(signature);

            doc.Descendants("DataSignature")
                .Single().Descendants("EncryptedKey")
                .Single().Value = ToBase64String(EncryptBytesRsa(signatureKey, rsaKey));

            doc.Save(manifestFilePath);
        }

        static byte[] EncryptBytesRsa(byte[] bytes, string publicRsaKeyXml)
        {
            byte[] encrypted;
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(publicRsaKeyXml);
                encrypted = rsa.Encrypt(bytes, true);
            }

            return encrypted;
        }

        public static byte[] DescryptBytesRsa(byte[] bytes, string privateRsaKeyXml)
        {
            byte[] decrypted;
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(privateRsaKeyXml);
                decrypted = rsa.Decrypt(bytes, true);
            }

            return decrypted;
        }

        static void Decrypt(string infoXmlFilePath, string privateRsaKeyXmlFilePath)
        {
            var folderPath = Path.GetDirectoryName(infoXmlFilePath);
            var privateRsaKey = ReadRsaKeyXmlFromFile(privateRsaKeyXmlFilePath);

            var xmlDoc = XDocument.Load(infoXmlFilePath);
            var fileName = xmlDoc.Root.XPathSelectElement("./FileName").Value;
            var filePath = Path.Combine(folderPath, fileName);

            var encryptedFileName = xmlDoc.Root.XPathSelectElement("./EncryptedFileName").Value;
            var encryptedFilePath = Path.Combine(folderPath, encryptedFileName);

            var aesKeyElement = xmlDoc.Root.XPathSelectElement("./DataEncryption/AESEncryptedKeyValue/Key");
            byte[] aesKey = DescryptBytesRsa(FromBase64String(aesKeyElement.Value), privateRsaKey);

            var aesIvElement = xmlDoc.Root.XPathSelectElement("./DataEncryption/AESEncryptedKeyValue/IV");
            byte[] aesIv = DescryptBytesRsa(Convert.FromBase64String(aesIvElement.Value), privateRsaKey);

            using (var aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = 128;
                aes.Key = aesKey;
                aes.IV = aesIv;
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var fsPlain = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var fsEncrypted = File.Open(encryptedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var cs = new CryptoStream(fsPlain, decryptor, CryptoStreamMode.Write))
                {
                    fsEncrypted.CopyTo(cs);
                }
            }
        }
    }
}
