﻿using Newtonsoft.Json;
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
        public const string Divider = "<databegin>";

        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
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
                var rawString = JsonConvert.SerializeObject(obj);
                Data = SecureEncoding.GetSecureEncoding(secretKey).Encrypt(rawString, iv);
            }
            catch (Exception e)
            {
                throw RemoteException.Get(RemoteExceptionData.DecryptionErrorCode, string.Empty, e);
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
                return JsonConvert.DeserializeObject<T>(decryptedRaw);
            }
            catch (Exception e)
            {
                throw RemoteException.Get(RemoteExceptionData.DecryptionErrorCode, string.Empty, e);
            }
        }

        public override string ToString() => Salt + Divider + Data;

        /// <summary>
        /// Create object by source string
        /// </summary>
        /// <param name="source">source string</param>
        /// <returns></returns>
        public static Encrypted<T> FromString(string source)
        {
            var arr = source.Split(new[] { Divider }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 2)
                throw RemoteException.Get(RemoteExceptionData.UnknownData);
            return new Encrypted<T>() {
                Data = arr[1],
                Salt = arr[0]
            };
        }
    }
}