using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace KuCoin.NET.Helpers
{
    /// <summary>
    /// GUID-key based symmetric string encryption and decryption
    /// </summary>
    public static class AesOperation
    {
        /// <summary>
        /// Encrypt a string with the given key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="plainText">The unencrypted text.</param>
        /// <param name="rawData">Receive the raw encrypted data, in bytes.</param>
        /// <returns>An encrypted, base-64 encoded text string.</returns>
        public static string EncryptString(Guid key, string plainText, out byte[] rawData)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key.ToByteArray();
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            rawData = array;
            return Convert.ToBase64String(array);
        }

        /// <summary>
        /// Decrypt a string using the given key
        /// </summary>
        /// <param name="key">The GUID key</param>
        /// <param name="cipherText">The encrypted, base-64 encoded text string.</param>
        /// <returns></returns>
        public static string DecryptString(Guid key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key.ToByteArray();
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
