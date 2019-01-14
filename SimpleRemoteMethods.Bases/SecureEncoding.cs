using PCLCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static PCLCrypto.WinRTCrypto;

namespace SimpleRemoteMethods
{
    /// <summary>
    /// Encryption/decryption of a custom data using the AES algorithm
    /// </summary>
    public class SecureEncoding
    {
        public readonly static Encoding TextEncoding = Encoding.UTF8;

        private static readonly Random Rand = new Random();
        private static Dictionary<string, SecureEncoding> Cached = new Dictionary<string, SecureEncoding>();
        private static object Locker_GetSecureEncoding = new object();

        public static SecureEncoding GetSecureEncoding(string key)
        {
            lock (Locker_GetSecureEncoding)
            {
                if (!Cached.ContainsKey(key))
                    Cached.Add(key, new SecureEncoding(key));
                return Cached[key];
            }
        }

        /// <summary>
        /// Create the initialization vector for encryption
        /// </summary>
        /// <param name="salt">Random data</param>
        /// <param name="secretKey">Secret key</param>
        /// <returns>Initialization vector</returns>
        public static byte[] CreateIV(string salt, string secretKey)
        {
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            var hash = BCrypt.Net.BCrypt.HashPassword(secretKey, salt);

            var secretKeyHashBytes = TextEncoding.GetBytes(hash);

            // Calculate offset of secretKeyHashBytes by summ of 
            // first secretKeyByte and first salt byte in proportion to 256
            var offset = (int)(((secretKeyHashBytes[0] + saltBytes[0]) / 256d) * 16);
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
                buff[i] = odd ? targetSecretKeyBytes[i / 2] : saltBytes[i / 2];
                odd = !odd;
            }

            return buff;
        }

        /// <summary>
        /// Create BCrypt salt
        /// </summary>
        /// <returns>Generate random chars</returns>
        public static string CreateSalt() => BCrypt.Net.BCrypt.GenerateSalt();

        private string _secretKey;

        /// <summary>
        /// Create SecureEncoding object
        /// </summary>
        /// <param name="secretKey">Secret key</param>
        public SecureEncoding(string secretKey)
        {
            _secretKey = secretKey;
        }

        public SecureEncoding() : this("secret))01234567") { }

        /// <summary>
        /// Encrypt data with initialization vector
        /// </summary>
        /// <param name="data">Target data</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>Encrypted data</returns>
        public string Encrypt(string data, byte[] iv)
        {
            return Encrypt(TextEncoding.GetBytes(data), iv);
        }

        /// <summary>
        /// Encrypt data with initialization vector
        /// </summary>
        /// <param name="data">Target data</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>Encrypted data in bytes</returns>
        public string Encrypt(byte[] data, byte[] iv)
        {
            return EncryptInternal(data, iv);
        }

        private string EncryptInternal(byte[] data, byte[] iv)
        {
            var algo = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
            var key = algo.CreateSymmetricKey(TextEncoding.GetBytes(_secretKey));
            return Convert.ToBase64String(CryptographicEngine.Encrypt(key, data, iv));
        }

        /// <summary>
        /// Decrypt data with initialization vector
        /// </summary>
        /// <param name="data">Target data</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>Decrypted data</returns>
        public string Decrypt(string data, byte[] iv)
        {
            var bytes = DecryptBytes(data, iv);
            return TextEncoding.GetString(bytes, 0, bytes.Length);
        }

        private byte[] DecryptBytesInternal(string data, byte[] iv)
        {
            var algo = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
            var key = algo.CreateSymmetricKey(TextEncoding.GetBytes(_secretKey));
            return CryptographicEngine.Decrypt(key, Convert.FromBase64String(data), iv);
        }

        /// <summary>
        /// Decrypt data with initialization vector
        /// </summary>
        /// <param name="data">Target data</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>Decrypted data in bytes</returns>
        public byte[] DecryptBytes(string data, byte[] iv)
        {
            return DecryptBytesInternal(data, iv);
        }
    }
}