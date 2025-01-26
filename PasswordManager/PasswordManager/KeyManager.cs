using System;
using System.IO;
using System.Security.Cryptography;

namespace PasswordManager
{
    public static class KeyManager
    {
        private static readonly string KeysDirectory = "keys";

        static KeyManager()
        {
            // Create "keys" folder if it doesn't exist
            if (!Directory.Exists(KeysDirectory))
            {
                Directory.CreateDirectory(KeysDirectory);
            }
        }

        public static string GenerateAesKey()
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();

            string keyId = $"AES_{Guid.NewGuid()}"; // Add AES prefix
            string keyPath = Path.Combine(KeysDirectory, $"{keyId}.bin");

            File.WriteAllBytes(keyPath, aes.Key);

            return keyId;
        }

        public static byte[] LoadAesKey(string keyId)
        {
            string keyPath = Path.Combine(KeysDirectory, $"{keyId}.bin");

            if (!File.Exists(keyPath))
            {
                throw new FileNotFoundException($"Key with ID {keyId} not found.");
            }

            return File.ReadAllBytes(keyPath);
        }

        public static string GenerateRsaKeys()
        {
            using var rsa = RSA.Create();
            rsa.KeySize = 2048;

            string keyId = $"RSA_{Guid.NewGuid()}"; // Add RSA prefix
            string privateKeyPath = Path.Combine(KeysDirectory, $"{keyId}_private.pem");
            string publicKeyPath = Path.Combine(KeysDirectory, $"{keyId}_public.pem");

            // Export the private key
            var privateKey = rsa.ExportRSAPrivateKey();
            File.WriteAllBytes(privateKeyPath, privateKey);

            // Export the public key
            var publicKey = rsa.ExportRSAPublicKey();
            File.WriteAllBytes(publicKeyPath, publicKey);

            return keyId;
        }

        public static byte[] LoadRsaPublicKey(string keyId)
        {
            string publicKeyPath = Path.Combine(KeysDirectory, $"{keyId}_public.pem");

            if (!File.Exists(publicKeyPath))
            {
                throw new FileNotFoundException($"Public key with ID {keyId} not found.");
            }

            return File.ReadAllBytes(publicKeyPath);
        }

        public static byte[] LoadRsaPrivateKey(string keyId)
        {
            string privateKeyPath = Path.Combine(KeysDirectory, $"{keyId}_private.pem");

            if (!File.Exists(privateKeyPath))
            {
                throw new FileNotFoundException($"Private key with ID {keyId} not found.");
            }

            return File.ReadAllBytes(privateKeyPath);
        }
    }
}