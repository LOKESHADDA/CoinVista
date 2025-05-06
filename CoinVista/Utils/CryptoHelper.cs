using System.Security.Cryptography;
using System.Text;

namespace CoinVista.Utils
{
    public static class CryptoHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("ThisIsA32ByteLongEncryptionKey!123"); // 32 chars
        private static readonly byte[] IV  = Encoding.UTF8.GetBytes("ThisIs16ByteIV!!"); // 16 chars

        public static string EncryptDecimal(decimal value)
        {
            var plainText = value.ToString("G29");
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);
            sw.Write(plainText);
            return Convert.ToBase64String(ms.ToArray());
        }

        public static decimal DecryptDecimal(string cipherText)
        {
            var bytes = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(bytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            var decryptedText = sr.ReadToEnd();
            return decimal.Parse(decryptedText);
        }
    }
}
