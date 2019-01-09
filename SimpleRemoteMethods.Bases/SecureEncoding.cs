using DevOne.Security.Cryptography.BCrypt;
using PCLCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static PCLCrypto.WinRTCrypto;

namespace SimpleRemoteMethods
{
    public class SecureEncoding
    {
        public readonly static Encoding TextEncoding = Encoding.UTF8;

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

        public static byte[] CreateIV(string salt, string secretKey)
        {
            var saltBytes = Convert.FromBase64String(salt);

            var secretKeyHashBytes = TextEncoding.GetBytes(BCryptHelper.HashPassword(secretKey, salt));

            //calculate offset of secretKeyHashBytes by summ of 
            //first secretKeyByte and first salt byte in proportion to 256
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

        public static string CreateSalt()
        {
            var buff = new byte[8];
            Rand.NextBytes(buff);
            return Convert.ToBase64String(buff);
        }

        private string _secretKey;

        public SecureEncoding(string secretKey)
        {
            _secretKey = secretKey;
        }

        public SecureEncoding() : this("secret))01234567") { }

        public string Encrypt(string data, byte[] iv)
        {
            return Encrypt(TextEncoding.GetBytes(data), iv);
        }

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

        public byte[] DecryptBytes(string data, byte[] iv)
        {
            return DecryptBytesInternal(data, iv);
        }
    }
}