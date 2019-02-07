using System;
using System.IO;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Class to encrypt/decrypt/serialize custom class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Encrypted<T>
    {
        private byte[] _salt;
        private readonly int _offset;

        /// <summary>
        /// Transfer data
        /// </summary>
        public byte[] Data { get; private set; }

        public Encrypted()
        {
            Statics.Settings();
        }

        /// <summary>
        /// Create new object from source data for encryption
        /// </summary>
        /// <param name="obj">Target object to encrypt</param>
        /// <param name="secretKey">Secret code</param>
        public Encrypted(T obj, string secretKey) : this()
        {
            try
            {
                _salt = SecureEncoding.CreateSalt();
                var iv = SecureEncoding.CreateIV(_salt, secretKey);
                var raw = Serialize(obj);
                using (var ms = new MemoryStream())
                {
                    var typeNameBytes = Encoding.UTF8.GetBytes(typeof(T).FullName);
                    if (typeNameBytes.Length > byte.MaxValue)
                        throw new RemoteException(ErrorCode.InternalServerError, "Type name bytes must be less than 256 bytes");
                    ms.WriteByte((byte)typeNameBytes.Length);
                    ms.Write(typeNameBytes, 0, typeNameBytes.Length);

                    if (_salt.Length > byte.MaxValue)
                        throw new RemoteException(ErrorCode.InternalServerError, "Salt must be less than 256 bytes");
                    ms.WriteByte((byte)_salt.Length);
                    ms.Write(_salt, 0, _salt.Length);

                    _offset = (int)ms.Position;

                    var encryptedData = SecureEncoding.GetSecureEncoding(secretKey).Encrypt(raw, iv);
                    ms.Write(encryptedData, 0, encryptedData.Length);

                    Data = ms.ToArray();
                }
            }
            catch (Exception e)
            {
                throw new RemoteException(ErrorCode.DecryptionErrorCode, "/", e);
            }
        }

        /// <summary>
        /// Create new object from encrypted raw data
        /// </summary>
        /// <param name="rawData">Data that contains type name, salt and encrypted serialized data</param>
        public Encrypted(byte[] rawData)
        {
            try
            {
                Data = rawData;
                var typenameLen = Data[0];
                var saltLen = Data[typenameLen + 1];
                _salt = new byte[saltLen];
                Array.Copy(Data, typenameLen + 2, _salt, 0, saltLen);
                _offset = typenameLen + saltLen + 2;
            }
            catch (Exception e)
            {
                throw new RemoteException(ErrorCode.DecryptionErrorCode, "/", e);
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
                var iv = SecureEncoding.CreateIV(_salt, secretKey);
                var secureEncoding = SecureEncoding.GetSecureEncoding(secretKey);
                var decryptedRaw = secureEncoding.Decrypt(_offset, Data, iv);
                return Deserialize(decryptedRaw);
            }
            catch (Exception e)
            {
                throw new RemoteException(ErrorCode.DecryptionErrorCode, "/", e);
            }
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
        public static bool IsClass(byte[] data)
        {
            if (data.Length == 0)
                return false;

            var t = typeof(T);
            if (data.Length < t.FullName.Length + 1)
                return false;

            var typeNameLen = data[0];
            if (data.Length < typeNameLen + 1)
                return false;

            var typeName = Encoding.UTF8.GetString(data, 1, typeNameLen);
            return typeName == t.FullName;
        }
    }
}
