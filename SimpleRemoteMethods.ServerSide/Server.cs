using SimpleRemoteMethods.Bases;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.ServerSide
{
    /// <summary>
    /// Server side logic
    /// </summary>
    /// <typeparam name="T">Class or interface that contains methods for remote use</typeparam>
    public class Server<T>
    {
        /// <summary>
        /// Create server
        /// </summary>
        /// <param name="objectMethods">Object that contains methods for remote use</param>
        public Server(T objectMethods, bool ssl, ushort port)
        {
            Methods = objectMethods;
            Ssl = ssl;
            Port = port;
        }

        /// <summary>
        /// Max synchronous connections
        /// </summary>
        public ushort MaxConcurrentCalls
        {
            get => _maxConcurrentCalls;
            set
            {
                if (_maxConcurrentCalls < 1)
                    throw new InvalidOperationException("MaxConcurrentCalls cannot be less than 1");
                _maxConcurrentCalls = value;
            }
        }

        /// <summary>
        /// Object that contains methods for remote use
        /// </summary>
        public T Methods { get; }

        /// <summary>
        /// User/password validator
        /// </summary>
        public IAuthenticationValidator AuthenticationValidator
        {
            get => _authenticationValidator;
            set => _authenticationValidator = value ?? throw new ArgumentNullException("AuthenticationValidator cannot be null");
        }
        /// <summary>
        /// Logic for user token destribution
        /// </summary>
        public ITokenDistributor TokenDistributor
        {
            get => _tokenDistributor;
            set => _tokenDistributor = value ?? throw new ArgumentNullException("AuthenticationValidator cannot be null");
        }
        /// <summary>
        /// Bruteforce checker by login
        /// </summary>
        public IBruteforceChecker BruteforceCheckerByLogin
        {
            get => _bruteforceCheckerByLogin;
            set => _bruteforceCheckerByLogin = value ?? throw new ArgumentNullException("BruteforceCheckerByLogin cannot be null");
        }
        /// <summary>
        /// Bruteforce checker by ip
        /// </summary>
        public IBruteforceChecker BruteforceCheckerByIpAddress
        {
            get => _bruteforceCheckerByIpAddress;
            set => _bruteforceCheckerByIpAddress = value ?? throw new ArgumentNullException("BruteforceCheckerByIpAddress cannot be null");
        }
        /// <summary>
        /// Object for tracking and checking request id
        /// </summary>
        public RequestIdChecker RequestChecker {
            get => _requestChecker;
            set => _requestChecker = value ?? throw new ArgumentNullException("RequestChecker cannot be null");
        }
        /// <summary>
        /// Use HTTPS
        /// </summary>
        public bool Ssl { get; }

        /// <summary>
        /// Server port
        /// </summary>
        public ushort Port { get; }

        /// <summary>
        /// Secret code that encrypts all data
        /// </summary>
        public string SecretCode { get; }

        /// <summary>
        /// Raises when need to write log
        /// </summary>
        public event EventHandler<LogRecordEventArgs> LogRecord;

        /// <summary>
        /// Raises before server start
        /// </summary>
        public event EventHandler<object> BeforeServerStart;

        /// <summary>
        /// Raises after server stopped
        /// </summary>
        public event EventHandler<object> AfterServerStopped;

        /// <summary>
        /// Current thread request
        /// </summary>
        public RequestContext CurrentRequestContext => _currentRequestContext;

        [ThreadStatic]
        private RequestContext _currentRequestContext;
        private HttpListener _listener;
        private bool _started = false;
        private bool _startedInternal = false;
        private ushort _connectionsCount = 0;
        private MethodsCaller<T> _caller = new MethodsCaller<T>();
        private ushort _maxConcurrentCalls = 20;
        private IAuthenticationValidator _authenticationValidator = new AuthenticationValidatorStub();
        private ITokenDistributor _tokenDistributor = new StandardTokenDistributor();
        private IBruteforceChecker _bruteforceCheckerByLogin = new StandardBruteforceChecker();
        private IBruteforceChecker _bruteforceCheckerByIpAddress = new StandardBruteforceChecker();
        private RequestIdChecker _requestChecker = new RequestIdChecker();

        /// <summary>
        /// Start http server asynchronously
        /// </summary>
        public async void StartAsync()
        {
            BeforeServerStart?.Invoke(this, this);

            _started = true;
            _listener = new HttpListener();
            _listener.Prefixes.Add(string.Format("{0}://+:{1}/", Ssl ? "https" : "http", Port));
            await StartAsyncInternal();

            AfterServerStopped?.Invoke(this, this);
        }

        private async Task StartAsyncInternal()
        {
            _startedInternal = true;
            await new Task(() =>
            {
                _listener.Start();
                while (_started && _startedInternal)
                {
                    var context = _listener.GetContext();
                    Task.Run(() => HandleContext(context));
                }
            },
            TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            _started = false;
            StopInternal();
        }

        private void StopInternal() => _listener.Stop();

        private void HandleContext(HttpListenerContext context)
        {
            HandleConnectionBegin();

            try
            {
                var clientIp = context.Request.RemoteEndPoint.ToString();

                RaiseLogRecord(LogType.Debug, string.Format("Client {0} connected...", clientIp));

                // Handle bruteforce suspicion by ip
                if (BruteforceCheckerByIpAddress.CheckIsBruteforce(clientIp))
                    throw RemoteException.Get(RemoteExceptionData.BruteforceSuspicion, "/", clientIp);

                var buff = new byte[context.Request.InputStream.Length];
                context.Request.InputStream.Read(buff, 0, buff.Length);
                var sourceStr = Encoding.UTF8.GetString(buff);

                // Ef encrypted data is Request
                if (Encrypted<Request>.IsClass(sourceStr))
                {
                    var request = Encrypted<Request>.FromString(sourceStr).Decrypt(SecretCode);
                    HandleRequest(request, context);
                }
                // If ecnrypted data is UserTokenRequest
                else if (Encrypted<UserTokenRequest>.IsClass(sourceStr))
                {
                    var request = Encrypted<UserTokenRequest>.FromString(sourceStr).Decrypt(SecretCode);
                    HandleUserTokenRequest(request, context);
                }
                else
                    throw RemoteException.Get(RemoteExceptionData.UnknownData, clientIp);
            }
            catch (RemoteException remoteException)
            {
                RaiseLogRecord(LogType.Info, remoteException);
                SendErrorResponse(remoteException.Data, context);
            }
            catch (Exception exception)
            {
                RaiseLogRecord(LogType.Error, exception);
                SendErrorResponse(new RemoteExceptionData(RemoteExceptionData.InternalServerError, exception.Message), context);
            }
            finally
            {
                HandleConnectionEnd();
            }
        }

        private void HandleRequest(Request request, HttpListenerContext context)
        {
            var clientIp = context.Request.RemoteEndPoint.ToString();

            // Handle request id fabrication
            if (request.RequestId != request.RequestIdRepeat ||
                !RequestChecker.IsNewRequest(request.RequestId))
                throw RemoteException.Get(RemoteExceptionData.RequestIdFabrication);

            RaiseLogRecord(LogType.Debug, string.Format("Ip {0} request id {1} normal...", clientIp, request.RequestId));

            // Authentication by token
            if (TokenDistributor.Authenticate(request.UserToken, out TokenInfo tokenInfo))
                _currentRequestContext = new RequestContext(request, tokenInfo.UserName, clientIp);
            else
                throw RemoteException.Get(RemoteExceptionData.UserTokenExpired, tokenInfo?.UserName ?? "/", clientIp);

            RaiseLogRecord(LogType.Debug, string.Format("{0} with ip {1} connected...", tokenInfo.UserName, clientIp));

            // Try to get method info and call
            var callInfo = _caller.Call(Methods, request.Method, request.Parameters, request.ReturnTypeName);

            // Check if method not exist
            if (callInfo.MethodNotFound)
                throw RemoteException.Get(RemoteExceptionData.MethodNotFound, tokenInfo.UserName, clientIp);

            // If target method threw an exception
            if (callInfo.CallException != null)
                throw RemoteException.Get(RemoteExceptionData.InternalServerError, tokenInfo.UserName, clientIp, callInfo.CallException);

            SendResponse(callInfo.Result, request.Method, context);

            RaiseLogRecord(LogType.Debug, string.Format("User {0} with ip {1} executed method {2}...", tokenInfo.UserName, clientIp, request.Method));
        }

        private void HandleUserTokenRequest(UserTokenRequest request, HttpListenerContext context)
        {
            var clientIp = context.Request.RemoteEndPoint.ToString();

            // Handle request id fabrication
            if (request.RequestId != request.RequestIdRepeat ||
                !RequestChecker.IsNewRequest(request.RequestId))
                throw RemoteException.Get(RemoteExceptionData.RequestIdFabrication);

            RaiseLogRecord(LogType.Debug, string.Format("Ip {0} request id {1} normal (user token request)...", clientIp, request.RequestId));

            // Authentication by login/password
            if (!AuthenticationValidator.Authenticate(request.Login, request.Password))
                throw RemoteException.Get(RemoteExceptionData.LoginOrPasswordInvalid, clientIp);

            RaiseLogRecord(LogType.Debug, string.Format("User {0} ({1}) password authentication success...", request.Login, clientIp));

            var newToken = TokenDistributor.RequestToken(request.Login, clientIp);

            SendUserTokenResponse(newToken, context);

            RaiseLogRecord(LogType.Info, string.Format("Server issued new token {0} for user {1} with ip {2}...", newToken, request.Login, clientIp));
        }

        private void SendResponse(object result, string method, HttpListenerContext context)
        {
            SendResponse(
                new Response() { Result = result, Method = method, ServerTime = DateTime.Now },
                context);
        }

        private void SendErrorResponse(RemoteExceptionData exceptionData, HttpListenerContext context)
        {
            SendErrorResponse(
                new ErrorResponse() { ErrorData = exceptionData },
                context);
        }

        private void SendUserTokenResponse(string newToken, HttpListenerContext context)
        {
            SendUserTokenResponse(
                new UserTokenResponse() { UserToken = newToken },
                context);
        }

        private void SendErrorResponse(ErrorResponse response, HttpListenerContext context)
        {
            var encryptedResponse = new Encrypted<ErrorResponse>(response, SecretCode);
            var bytes = Encoding.UTF8.GetBytes(encryptedResponse.ToString());
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
        }

        private void SendResponse(Response response, HttpListenerContext context)
        {
            var encryptedResponse = new Encrypted<Response>(response, SecretCode);
            var bytes = Encoding.UTF8.GetBytes(encryptedResponse.ToString());
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
        }

        private void SendUserTokenResponse(UserTokenResponse tokenResponse, HttpListenerContext context)
        {
            var encryptedResponse = new Encrypted<UserTokenResponse>(tokenResponse, SecretCode);
            var bytes = Encoding.UTF8.GetBytes(encryptedResponse.ToString());
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
        }

        private void HandleConnectionBegin()
        {
            _connectionsCount++;
            if (_connectionsCount >= MaxConcurrentCalls)
            {
                RaiseLogRecord(LogType.Info, string.Format("Too many connections ({0}). Server suspend.", _connectionsCount));
                StopInternal();
            }
        }

        private async void HandleConnectionEnd()
        {
            _connectionsCount--;
            if (_connectionsCount < MaxConcurrentCalls && !_startedInternal && _started)
            {
                RaiseLogRecord(LogType.Info, string.Format("Connections count normal. Server continues to work.", _connectionsCount));
                await StartAsyncInternal();
            }
        }

        private void RaiseLogRecord(LogType type, Exception exception) => LogRecord?.Invoke(this, new LogRecordEventArgs(type, exception));

        private void RaiseLogRecord(LogType type, string message) => LogRecord?.Invoke(this, new LogRecordEventArgs(type, message));
    }
}
