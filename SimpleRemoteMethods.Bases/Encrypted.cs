using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

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

        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

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
            //do nothing
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
                Salt = SecureEncoding.CreateSalt();
                var iv = SecureEncoding.CreateIV(Salt, secretKey);
                var rawString = JsonConvert.SerializeObject(obj, SerializerSettings);
                Data = SecureEncoding.GetSecureEncoding(secretKey).Encrypt(rawString, iv);
            }
            catch (Exception e)
            {
                throw RemoteException.Get(RemoteExceptionData.DecryptionErrorCode, e.Message, e);
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
                var iv = SecureEncoding.CreateIV(Salt, secretKey);
                var secureEncoding = SecureEncoding.GetSecureEncoding(secretKey);
                var decryptedRaw = secureEncoding.Decrypt(Data, iv);
                return JsonConvert.DeserializeObject<T>(decryptedRaw, SerializerSettings);
            }
            catch (Exception e)
            {
                throw RemoteException.Get(RemoteExceptionData.DecryptionErrorCode, e.Message, e);
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

        public static bool IsClass(string source)
        {
            var t = typeof(T);
            var arr = source.Split(new[] { DividerSaltBegin }, StringSplitOptions.RemoveEmptyEntries);
            return arr[0] == t.Name;
        }
    }
}
