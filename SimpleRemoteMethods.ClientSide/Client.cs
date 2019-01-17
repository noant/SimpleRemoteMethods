﻿using SimpleRemoteMethods.Bases;
using System;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.ClientSide
{
    /// <summary>
    /// Client to access SimpleRemoteMethods server
    /// </summary>
    public class Client
    {
        static Client()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        /// <summary>
        /// Handling https server certificate (by default remote there is no validation of remote certificate)
        /// (reference from System.Net.Security.ServicePointManager.ServerCertificateValidationCallback)
        /// </summary>
        public static RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get => ServicePointManager.ServerCertificateValidationCallback;
            set => ServicePointManager.ServerCertificateValidationCallback = value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host">Host name of server</param>
        /// <param name="port">Server host</param>
        /// <param name="ssl">Use HTTPS</param>
        /// <param name="secretKey">Secret code to encrypt data</param>
        /// <param name="login">User login name</param>
        /// <param name="password">User password</param>
        public Client(string host, ushort port, bool ssl, string secretKey, string login, string password)
        {
            Host = host;
            Port = port;
            Ssl = ssl;
            SecretKey = secretKey;
            Login = login;
            Password = password;

            CallUri = new Uri(string.Format(@"{0}://{1}:{2}", ssl ? "https" : "http", host, port));
        }

        /// <summary>
        /// Uri of server
        /// </summary>
        public Uri CallUri { get; }

        /// <summary>
        /// Server host name
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Server port
        /// </summary>
        public ushort Port { get; }

        /// <summary>
        /// Use HTTPS
        /// </summary>
        public bool Ssl { get; }

        /// <summary>
        /// Secret code to encrypt data
        /// </summary>
        public string SecretKey { get; }

        /// <summary>
        /// User login
        /// </summary>
        public string Login { get; }

        /// <summary>
        /// User password
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Server last call DateTime
        /// </summary>
        public DateTime LastCallServerTime { get; private set; }

        /// <summary>
        /// Current authentication user token 
        /// </summary>
        public string CurrentUserToken { get; private set; }

        /// <summary>
        /// Call remote method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="methodName">Target method name</param>
        /// <param name="parameters">Input parameters</param>
        /// <returns>Return Task<T></returns>
        public async Task<T> CallMethod<T>(string methodName, params object[] parameters)
        {
            if (string.IsNullOrEmpty(CurrentUserToken))
                await RefreshToken();
            try
            {
                return await CallMethodInternal<T>(methodName, parameters);
            }
            catch (RemoteException e) when (e.Data.Code == RemoteExceptionData.UserTokenExpired)
            {
                await RefreshToken();
                return await CallMethodInternal<T>(methodName, parameters);
            }
        }

        /// <summary>
        /// Call remote method
        /// </summary>
        /// <param name="methodName">Target method name</param>
        /// <param name="parameters">Input parameters</param>
        public async Task CallMethod(string methodName, params object[] parameters)
        {
            if (string.IsNullOrEmpty(CurrentUserToken))
                await RefreshToken();
            try
            {
                await CallMethodInternal(methodName, parameters);
            }
            catch (RemoteException e) when (e.Data.Code == RemoteExceptionData.UserTokenExpired)
            {
                await RefreshToken();
                await CallMethodInternal(methodName, parameters);
            }
        }

        private async Task<Response> SendRequest(string methodName, string returnTypeName, params object[] parameters)
        {
            var request = PrepareRequest(methodName, returnTypeName, parameters);
            var response = await HttpUtils.SendRequest(CallUri, request, SecretKey);
            LastCallServerTime = response.ServerTime;
            return response;
        }

        private async Task<T> CallMethodInternal<T>(string methodName, params object[] parameters)
        {
            var response = await SendRequest(methodName, typeof(T).FullName, parameters);
            var result = response.Result;
            if (result != null)
                return (T)result;
            else return default(T);
        }

        private async Task CallMethodInternal(string methodName, params object[] parameters)
        {
            await SendRequest(methodName, typeof(void).FullName, parameters);
        }

        private async Task RefreshToken()
        {
            var request = new UserTokenRequest();
            request.Login = Login;
            request.Password = Password;
            request.RequestId =
                request.RequestIdRepeat = Guid.NewGuid().ToString();

            var response = await HttpUtils.SendUserTokenRequest(CallUri, request, SecretKey);
            CurrentUserToken = response.UserToken;
        }

        private Request PrepareRequest(string methodName, string methodReturnParam, params object[] parameters)
        {
            var request = new Request();
            request.Method = methodName;
            request.Parameters = parameters;
            request.RequestId =
                request.RequestIdRepeat = Guid.NewGuid().ToString();
            request.ReturnTypeName = methodReturnParam;
            request.UserToken = CurrentUserToken;

            return request;
        }
    }
}