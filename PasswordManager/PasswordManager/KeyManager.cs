using System.IO;
using System.Security.Cryptography;

namespace PasswordManager
{
    public static class KeyManager
    {
        public static void GenerateAesKey(string filePath)
        {
            using var aes = Aes.Create();
            File.WriteAllBytes(filePath, aes.Key);
        }

        public static byte[] LoadAesKey(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
        public static void GenerateRsaKeys(string filePath) { /* Tworzenie klucza RSA */ }
    }
}