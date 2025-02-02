﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager
{
    public static class CryptoHelper
    {
        public static byte[] EncryptWithAes(string plainText, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);

            sw.Write(plainText);
            sw.Close();

            byte[] encryptedData = ms.ToArray();
            byte[] result = new byte[aes.IV.Length + encryptedData.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedData, 0, result, aes.IV.Length, encryptedData.Length);

            return result;
        }

        public static string DecryptWithAes(byte[] encryptedData, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;

            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] ciphertext = new byte[encryptedData.Length - iv.Length];

            Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(encryptedData, iv.Length, ciphertext, 0, ciphertext.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(ciphertext);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        public static string EncryptWithRsa(string plainText, byte[] publicKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(publicKey, out _);

            byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(encryptedData);
        }

        public static string DecryptWithRsa(string encryptedText, byte[] privateKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);

            byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(encryptedText), RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}
