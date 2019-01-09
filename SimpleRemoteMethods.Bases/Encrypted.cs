using Lazurite.IOC;
using Lazurite.Logging;
using Lazurite.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace SimpleRemoteMethods.Bases
{
    public class Encrypted<T>
    {
        public const string Divider = "<databegin>";

        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public string Salt { get; set; }

        public string Data { get; set; }

        public Encrypted()
        {
            //do nothing
        }

        public Encrypted(T obj, string secretKey) : this()
        {
            try
            {
                Salt = SecureEncoding.CreateSalt();
                var iv = SecureEncoding.CreateIV(Salt, secretKey);
                var rawString = JsonConvert.SerializeObject(obj);
                Data = SecureEncoding.GetSecureEncoding(secretKey).Encrypt(rawString, iv);
            }
            catch (Exception e)
            {
                RemoteException.Throw(RemoteExceptionData.DecryptionErrorCode, string.Empty, e);
            }
        }

        public T Decrypt(string secretKey)
        {
            try
            {
                var iv = SecureEncoding.CreateIV(Salt, secretKey);
                var secureEncoding = SecureEncoding.GetSecureEncoding(secretKey);
                var decryptedRaw = secureEncoding.Decrypt(Data, iv);
                return JsonConvert.DeserializeObject<T>(decryptedRaw);
            }
            catch (Exception e)
            {
                RemoteException.Throw(RemoteExceptionData.DecryptionErrorCode, string.Empty, e);
            }
        }

        public override string ToString() => Salt + Divider + Data;

        public static Encrypted<T> FromString(string source)
        {
            var arr = source.Split(new[] { Divider }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 2)
                RemoteException.Throw(RemoteExceptionData.UnknownData);
            return new Encrypted<T>() {
                Data = arr[1],
                Salt = arr[0]
            };
        }
    }
}
