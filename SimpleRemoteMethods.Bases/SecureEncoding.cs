using Konscious.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SimpleRemoteMethods
{
    /// <summary>
    /// Encryption/decryption of a custom data using the AES algorithm
    /// </summary>
    public class SecureEncoding
    {
        public readonly static Encoding TextEncoding = Encoding.UTF8;

        private static readonly RNGCryptoServiceProvider RNGCryptoServiceProvider = new RNGCryptoServiceProvider();
        private static Dictionary<string, SecureEncoding> Cached = new Dictionary<string, SecureEncoding>();
        private static readonly object Locker_GetSecureEncoding = new object();

        /// <summary>
        /// Get cached object
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static SecureEncoding GetSecureEncoding(string key)
        {
            lock (Locker_GetSecureEncoding)
            {
                SecureEncoding retVal = null;
                if (!Cached.ContainsKey(key))
                    Cached.Add(key, retVal = new SecureEncoding(key));
                else retVal = Cached[key];
                return retVal;
            }
        }

        /// <summary>
        /// Create the initialization vector for encryption
        /// </summary>
        /// <param name="salt">Random data</param>
        /// <param name="secretKey">Secret key</param>
        /// <returns>Initialization vector</returns>
        public static byte[] CreateIV(byte[] salt, string secretKey)
        {
            var secretKeyHashBytes = CreateHash(TextEncoding.GetBytes(secretKey), salt);

            // Calculate offset of secretKeyHashBytes by summ of 
            // first secretKeyByte and first salt byte in proportion to 256
            var offset = (int)(((secretKeyHashBytes[0] + salt[0]) / 256d) * 16);
            if (offset > 16)
                offset -= 16;
            var targetSecretKeyBytes = new byte[8];
            for (var i = 0; i < targetSecretKeyBytes.Length; i++)
            {
                var index = offset + i;
                if (index >= 8)
                    index -= 8;
                targetSecretKeyBytes[i] = secretKeyHashBytes[index];
            }
            var buff = new byte[16];
            var odd = secretKeyHashBytes[0] > 123; //half byte
            for (var i = 0; i < buff.Length; i++)
            {
                buff[i] = odd ? targetSecretKeyBytes[i / 2] : salt[i / 2];
                odd = !odd;
            }

            return buff;
        }

        /// <summary>
        /// Create salt
        /// </summary>
        /// <returns>Random 32 bytes</returns>
        public static byte[] CreateSalt() {
            var salt = new byte[32];
            RNGCryptoServiceProvider.GetBytes(salt);
            return salt;
        }

        /// <summary>
        /// Create SecureEncoding object
        /// </summary>
        /// <param name="secretKey">Secret key</param>
        public SecureEncoding(string secretKey)
        {
            _secretKey = secretKey;
            _secretKeyBytes = TextEncoding.GetBytes(_secretKey);
        }

        public SecureEncoding() : this("secret))01234567") { }

        /// <summary>
        /// Encrypt data with initialization vector
        /// </summary>
        /// <param name="data">Target data</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>Encrypted data</returns>
        public byte[] Encrypt(byte[] data, byte[] iv) => EncryptBytesInternal(data, iv);

        /// <summary>
        /// Encrypt data with initialization vector
        /// </summary>
        /// <param name="data">Target data</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>Encrypted data in base64</returns>
        public string Encrypt(string data, byte[] iv)
        {
            var dataBytes = TextEncoding.GetBytes(data);
            return Convert.ToBase64String(EncryptBytesInternal(dataBytes, iv));
        }

        /// <summary>
        /// Decrypt data with initialization vector
        /// </summary>
        /// <param name="base64data">Target data</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>Decrypted data</returns>
        public string Decrypt(string base64data, byte[] iv)
        {
            var dataBytes = Convert.FromBase64String(base64data);
            var bytes = Decrypt(dataBytes, iv);
            return TextEncoding.GetString(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Decrypt data with initialization vector
        /// </summary>
        /// <param name="data">Target data</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>Decrypted data in bytes</returns>
        public byte[] Decrypt(byte[] data, byte[] iv) => DecryptBytesInternal(0, data, iv);

        /// <summary>
        /// Decrypt data with initialization vector
        /// </summary>
        /// <param name="offset">Offset from encrypted data begins</param>
        /// <param name="data">Target data</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>Decrypted data in bytes</returns>
        public byte[] Decrypt(int offset, byte[] data, byte[] iv) => DecryptBytesInternal(offset, data, iv);

        #region private

        private readonly string _secretKey;
        private readonly byte[] _secretKeyBytes;

        private static byte[] CreateHash(byte[] salt, byte[] data)
        {
            using (var hashCreator = new HMACBlake2B(salt, 512))
                return hashCreator.ComputeHash(data);
        }

        private byte[] DecryptBytesInternal(int offset, byte[] data, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _secretKeyBytes;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream(data, offset, data.Length - offset))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var msReader = new MemoryStream())
                {
                    cs.CopyTo(msReader);
                    return msReader.ToArray();
                }
            }
        }

        private byte[] EncryptBytesInternal(byte[] data, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _secretKeyBytes;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        cs.Write(data, 0, data.Length);
                    return ms.ToArray();
                }
            }
        }

        #endregion
    }
}