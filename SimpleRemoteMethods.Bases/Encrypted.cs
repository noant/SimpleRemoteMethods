using System;
using System.IO;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Class to encrypt/decrypt/serialize custom class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Encrypted<T>
    {
        public const string DividerDataBegin = "<databegin>";
        public const string DividerSaltBegin = "<saltbegin>";
        
        /// <summary>
        /// Random data for encryption
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// Encrypted data
        /// </summary>
        public string Data { get; set; }

        public Encrypted()
        {
            Statics.Settings();
        }

        /// <summary>
        /// Create new object
        /// </summary>
        /// <param name="obj">Target object to encrypt</param>
        /// <param name="secretKey">Secret code</param>
        public Encrypted(T obj, string secretKey) : this()
        {
            try
            {
                var saltBytes = SecureEncoding.CreateSalt();
                Salt = Convert.ToBase64String(saltBytes);
                var iv = SecureEncoding.CreateIV(saltBytes, secretKey);
                var raw = Serialize(obj);
                Data = SecureEncoding.GetSecureEncoding(secretKey).Encrypt(raw, iv);
            }
            catch (Exception e)
            {
                throw RemoteException.Get(RemoteExceptionData.DecryptionErrorCode, "/", e);
            }
        }

        /// <summary>
        /// Decrypt data and create object
        /// </summary>
        /// <param name="secretKey">Secret code</param>
        /// <returns>Target class object</returns>
        public T Decrypt(string secretKey)
        {
            try
            {
                var saltBytes = Convert.FromBase64String(Salt);
                var iv = SecureEncoding.CreateIV(saltBytes, secretKey);
                var secureEncoding = SecureEncoding.GetSecureEncoding(secretKey);
                var decryptedRaw = secureEncoding.DecryptBytes(Data, iv);
                return Deserialize(decryptedRaw);
            }
            catch (Exception e)
            {
                throw RemoteException.Get(RemoteExceptionData.DecryptionErrorCode, "/", e);
            }
        }

        public override string ToString() => typeof(T).Name + DividerSaltBegin +  Salt + DividerDataBegin + Data;

        /// <summary>
        /// Create object by source string
        /// </summary>
        /// <param name="source">source string</param>
        /// <returns></returns>
        public static Encrypted<T> FromString(string source)
        {
            var arr = source.Split(new[] { DividerDataBegin, DividerSaltBegin }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 3)
                throw RemoteException.Get(RemoteExceptionData.UnknownData);
            return new Encrypted<T>() {
                Data = arr[2],
                Salt = arr[1]
            };
        }

        private byte[] Serialize(T obj)
        {
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, obj);
                ms.Position = 0;
                var buff = new byte[ms.Length];
                ms.Read(buff, 0, (int)ms.Length);
                return buff;
            }
        }

        private T Deserialize(byte[] raw)
        {
            using (var ms = new MemoryStream(raw))
                return ProtoBuf.Serializer.Deserialize<T>(ms);
        }

        /// <summary>
        /// Determine whether the encrypted data is the current T-class
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsClass(string source)
        {
            var t = typeof(T);
            var arr = source.Split(new[] { DividerSaltBegin }, StringSplitOptions.RemoveEmptyEntries);
            return arr[0] == t.Name;
        }
    }
}
