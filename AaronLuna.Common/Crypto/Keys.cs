namespace AaronLuna.Common.Crypto
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    using Result;
        
    public static class Keys
    {
        public static Result GenerateRsaPublicAndPrivateKeyFiles(string keyFolderPath)
        {
            var publicKeyFilePath = Path.Combine(keyFolderPath, "id_rsa_pub.xml");
            var privateKeyFilePath = Path.Combine(keyFolderPath, "id_rsa.xml");

            try
            {
                var rsa = new RSACryptoServiceProvider(2048);
                var publicKey = rsa.ToXmlString(false);
                var privateKey = rsa.ToXmlString(true);

                using (StreamWriter sw = File.CreateText(publicKeyFilePath))
                {
                    sw.Write(publicKey);
                }

                using (StreamWriter sw = File.CreateText(privateKeyFilePath))
                {
                    sw.Write(privateKey);
                }
            }
            catch (Exception ex)
            {
                return Result.Fail($"{ex.Message} {ex.GetType()}");
            }

            return Result.Ok();
        }
    }
}
