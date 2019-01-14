using SimpleRemoteMethods.Bases;
using System;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.ClientSide
{
    /// <summary>
    /// Client to access SimpleRemoteMethods server
    /// </summary>
    public class Client
    {
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

        private string _myToken;

        private Request PrepareRequest(string methodName, string methodReturnParam, params object[] parameters)
        {
            var request = new Request();
            request.Method = methodName;
            request.Parameters = parameters;
            request.RequestId =
                request.RequestIdRepeat = Guid.NewGuid().ToString();
            request.ReturnTypeName = methodReturnParam;
            request.UserToken = _myToken;

            return request;
        }

        /// <summary>
        /// Call remote method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="methodName">Target method name</param>
        /// <param name="parameters">Input parameters</param>
        /// <returns>Return Task<T></returns>
        public async Task<T> CallMethod<T>(string methodName, params object[] parameters)
        {
            if (string.IsNullOrEmpty(_myToken))
                await RefreshToken();

            var request = PrepareRequest(methodName, typeof(T).FullName, parameters);
            var response = await HttpUtils.SendRequest(CallUri, request, SecretKey);
            LastCallServerTime = response.ServerTime;

            if (response.Result == null)
                return default(T);
            return (T)response.Result;
        }

        /// <summary>
        /// Call remote method
        /// </summary>
        /// <param name="methodName">Target method name</param>
        /// <param name="parameters">Input parameters</param>
        public async void CallMethod(string methodName, params object[] parameters)
        {
            if (string.IsNullOrEmpty(_myToken))
                await RefreshToken();

            var request = PrepareRequest(methodName, typeof(void).FullName, parameters);
            var response = await HttpUtils.SendRequest(CallUri, request, SecretKey);
            LastCallServerTime = response.ServerTime;
        }

        private async Task RefreshToken()
        {
            var request = new UserTokenRequest();
            request.Login = Login;
            request.Password = Password;
            request.RequestId =
                request.RequestIdRepeat = Guid.NewGuid().ToString();

            var response = await HttpUtils.SendUserTokenRequest(CallUri, request, SecretKey);
            _myToken = response.UserToken;
        }
    }
}
