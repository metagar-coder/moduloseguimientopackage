using moduloseguimiento.API.Services.Interfaces;
using System.Security.Cryptography;

namespace moduloseguimiento.API.Services
{

    public class EncryptService : IEncrypt
    {

        private static readonly int iterations = 1000;

        public string SEncrypt(string input, string password)
        {
            byte[] encrypted;
            byte[] IV;
            byte[] Salt = GetSalt();
            byte[] Key = CreateKey(password, Salt);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;

                aesAlg.GenerateIV();
                IV = aesAlg.IV;

                aesAlg.CreateEncryptor();
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new(csEncrypt))
                {
                    swEncrypt.Write(input);
                }
                encrypted = msEncrypt.ToArray();
            }

            byte[] combinedIvSaltCt = new byte[Salt.Length + IV.Length + encrypted.Length];
            Array.Copy(Salt, 0, combinedIvSaltCt, 0, Salt.Length);
            Array.Copy(IV, 0, combinedIvSaltCt, Salt.Length, IV.Length);
            Array.Copy(encrypted, 0, combinedIvSaltCt, Salt.Length + IV.Length, encrypted.Length);

            return Convert.ToBase64String(combinedIvSaltCt.ToArray());
        }

        private byte[] CreateKey(string password, byte[] salt)
        {
            using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations);
            return rfc2898DeriveBytes.GetBytes(32);
        }

        private byte[] GetSalt()
        {
            var salt = new byte[32];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }
            return salt;
        }

    }

}
