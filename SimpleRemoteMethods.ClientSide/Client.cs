﻿using SimpleRemoteMethods.Bases;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
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

        private SafeHttpClient _httpClient;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host">Host name of server</param>
        /// <param name="port">Server host</param>
        /// <param name="ssl">Use HTTPS</param>
        /// <param name="secretKey">Secret code to encrypt data</param>
        /// <param name="login">User login name</param>
        /// <param name="password">User password</param>
        /// <param name="connectionTimeout">Connection timeout (default is 1 minute)</param>
        /// <param name="leaseTimeout">Connection lease timeout (default is 1 hour)</param>
        public Client(string host, ushort port, bool ssl, string secretKey, string login, string password, TimeSpan connectionTimeout = default(TimeSpan), TimeSpan leaseTimeout = default(TimeSpan))
        {
            Host = host ?? throw new RemoteException(ErrorCode.DecryptionErrorCode, "Host cannot be null");
            Port = port;
            Ssl = ssl;
            SecretKey = secretKey ?? throw new RemoteException(ErrorCode.DecryptionErrorCode, "SecretCode cannot be null");
            Login = login;
            Password = password;

            CallUri = new Uri($@"{(ssl ? "https" : "http")}://{host}:{port}");

            if (connectionTimeout == default(TimeSpan))
                connectionTimeout = TimeSpan.FromMinutes(1);

            if (leaseTimeout == default(TimeSpan))
                leaseTimeout = TimeSpan.FromHours(1);

            _httpClient = new SafeHttpClient(CallUri, connectionTimeout, leaseTimeout);
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
        /// Get connection timeout
        /// </summary>
        public TimeSpan ConnectionTimeout => _httpClient.ConnectionTimeout;

        /// <summary>
        /// Get lease timeout
        /// </summary>
        public TimeSpan LeaseTimeout => _httpClient.LeaseTimeout;

        /// <summary>
        /// Access to low-level request
        /// </summary>
        public event EventHandler<TaggedEventArgs<HttpRequestMessage>> UserRequest;

        /// <summary>
        /// Access to low-level response
        /// </summary>
        public event EventHandler<TaggedEventArgs<HttpResponseMessage>> ServerResponse;

        /// <summary>
        /// Raises when server issued new token for this client
        /// </summary>
        public event EventHandler<TaggedEventArgs<UserTokenResponse>> NewUserTokenIssued;

        /// <summary>
        /// Raises when server throws exception on client with code "2" (UserTokenExpired)
        /// </summary>
        public event EventHandler<TaggedEventArgs<string>> UserTokenExpired;

        /// <summary>
        /// Raises when exception was thrown and RemoteExceptionCode is not InternalServerError and not UserTokenExpired (IsConnectionNormal == false)
        /// </summary>
        public event EventHandler<TaggedEventArgs<RemoteException>> ConnectionError;

        /// <summary>
        /// Raises when IsConnectionNormal was setted to True 
        /// </summary>
        public event EventHandler<Client> ConnectionNormal;

        /// <summary>
        /// True if last remote method call was successful
        /// </summary>
        public bool IsConnectionNormal { get; private set; }

        /// <summary>
        /// Call remote method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="methodName">Target method name</param>
        /// <param name="parameters">Input parameters</param>
        /// <returns>Return Task<T></returns>
        public async Task<T> CallMethod<T>(string methodName, object[] parameters = null)
        {
            bool exceptionThrown = false;
            if (string.IsNullOrEmpty(CurrentUserToken))
                await RefreshToken();
            try
            {
                return await CallMethodInternal<T>(methodName, parameters);
            }
            catch (RemoteException e) when (e.Data.Code == ErrorCode.UserTokenExpired)
            {
                UserTokenExpired?.Invoke(this, new TaggedEventArgs<string>(CurrentUserToken));
                await RefreshToken();
                return await CallMethodInternal<T>(methodName, parameters);
            }
            catch (RemoteException e) when (e.Code != ErrorCode.InternalServerError)
            {
                exceptionThrown = true;
                RaiseConnectionError(e);
                throw e;
            }
            finally
            {
                if (!exceptionThrown)
                    RaiseConnectionNormal();
            }
        }

        /// <summary>
        /// Call remote method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="methodName">Target method name</param>
        /// <param name="parameters">Input parameters</param>
        /// <returns>Return Task<T></returns>
        public async Task<T[]> CallMethodArray<T>(string methodName, object[] parameters = null)
        {
            bool exceptionThrown = false;
            if (string.IsNullOrEmpty(CurrentUserToken))
                await RefreshToken();
            try
            {
                return await CallMethodArrayInternal<T>(methodName, parameters);
            }
            catch (RemoteException e) when (e.Data.Code == ErrorCode.UserTokenExpired)
            {
                UserTokenExpired?.Invoke(this, new TaggedEventArgs<string>(CurrentUserToken));
                await RefreshToken();
                return await CallMethodArrayInternal<T>(methodName, parameters);
            }
            catch (RemoteException e) when (e.Code != ErrorCode.InternalServerError)
            {
                exceptionThrown = true;
                RaiseConnectionError(e);
                throw e;
            }
            finally
            {
                if (!exceptionThrown)
                    RaiseConnectionNormal();
            }
        }

        /// <summary>
        /// Call remote method
        /// </summary>
        /// <param name="methodName">Target method name</param>
        /// <param name="parameters">Input parameters</param>
        public async Task CallMethod(string methodName, object[] parameters = null)
        {
            bool exceptionThrown = false;
            if (string.IsNullOrEmpty(CurrentUserToken))
                await RefreshToken();
            try
            {
                await CallMethodInternal(methodName, parameters);
            }
            catch (RemoteException e) when (e.Code == ErrorCode.UserTokenExpired)
            {
                await RefreshToken();
                await CallMethodInternal(methodName, parameters);
            }
            catch (RemoteException e) when (e.Code != ErrorCode.InternalServerError)
            {
                exceptionThrown = true;
                RaiseConnectionError(e);
                throw e;
            }
            finally
            {
                if (!exceptionThrown)
                    RaiseConnectionNormal();
            }
        }

        private async Task<Response> SendRequest(string methodName, string returnTypeName, object[] parameters)
        {
            var request = PrepareRequest(methodName, returnTypeName, parameters);
            var response = await HttpUtils.SendRequest(_httpClient, request, SecretKey, RaiseUserRequest, RaiseServerResponse);
            LastCallServerTime = response.ServerTime;
            return response;
        }

        private async Task<T> CallMethodInternal<T>(string methodName, object[] parameters)
        {
            var response = await SendRequest(methodName, typeof(T).FullName, parameters);
            var result = response.Result ?? response.ResultArray;
            if (result != null)
                return (T)result;
            else return default(T);
        }

        private async Task<T[]> CallMethodArrayInternal<T>(string methodName, object[] parameters)
        {
            var response = await SendRequest(methodName, typeof(T[]).FullName, parameters);
            var result = response.ResultArray;
            if (result != null)
                return result.Cast<T>().ToArray();
            else if (response.IsEmptyArray ?? false)
                return new T[0];
            else
                return default(T[]);
        }

        private async Task CallMethodInternal(string methodName, object[] parameters)
        {
            await SendRequest(methodName, typeof(void).FullName, parameters);
        }

        private async Task RefreshToken()
        {
            var exceptionThrown = false;
            try
            {
                var request = new UserTokenRequest();
                request.Login = Login;
                request.Password = Password;
                request.RequestId =
                    request.RequestIdRepeat = Guid.NewGuid().ToString();

                var response = await HttpUtils.SendUserTokenRequest(_httpClient, request, SecretKey);
                NewUserTokenIssued?.Invoke(this, new TaggedEventArgs<UserTokenResponse>(response));
                CurrentUserToken = response.UserToken;
            }
            catch (RemoteException e) when (e.Code != ErrorCode.InternalServerError)
            {
                exceptionThrown = true;
                RaiseConnectionError(e);
                throw e;
            }
            finally
            {
                if (!exceptionThrown)
                    RaiseConnectionNormal();
            }
        }

        private Request PrepareRequest(string methodName, string methodReturnParam, object[] parameters)
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

        private void RaiseUserRequest(HttpRequestMessage request) =>
            UserRequest?.Invoke(this, new TaggedEventArgs<HttpRequestMessage>(request));

        private void RaiseServerResponse(HttpResponseMessage response) =>
            ServerResponse?.Invoke(this, new TaggedEventArgs<HttpResponseMessage>(response));

        private void RaiseConnectionError(RemoteException exception)
        {
            IsConnectionNormal = false;
            ConnectionError?.Invoke(this, new TaggedEventArgs<RemoteException>(exception));
        }

        private void RaiseConnectionNormal()
        {
            if (!IsConnectionNormal)
            {
                IsConnectionNormal = true;
                ConnectionNormal?.Invoke(this, this);
            }
        }
    }
}