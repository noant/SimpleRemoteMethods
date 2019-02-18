using SimpleRemoteMethods.Bases;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.ServerSide
{
    /// <summary>
    /// Server side logic
    /// </summary>
    /// <typeparam name="T">Class or interface that contains contracts for remote use</typeparam>
    public class Server<T> : IDisposable
    {
        [ThreadStatic]
        private static RequestContext _currentRequestContext;

        /// <summary>
        /// Current thread request context
        /// </summary>
        public static RequestContext CurrentRequestContext => _currentRequestContext;

        /// <summary>
        /// Create server
        /// </summary>
        /// <param name="objectMethods">Object that contains methods for remote use</param>
        /// <param name="ssl">Use HTTPS</param>
        /// <param name="port">Server port</param>
        /// <param name="secretCode">Secret code for AES encryption</param>
        /// <param name="taskLimit">Max concurrent threads for server threading</param>
        public Server(T objectMethods, bool ssl, ushort port, string secretCode, ushort taskLimit = 4)
        {
            Methods = objectMethods;
            Ssl = ssl;
            Port = port;
            SecretCode = secretCode;
            _taskQueue = new TaskQueue(taskLimit);
        }

        /// <summary>
        /// Max synchronous connections
        /// </summary>
        public ushort MaxConcurrentCalls
        {
            get => _maxConcurrentCalls;
            set
            {
                if (value < 1)
                {
                    throw new InvalidOperationException("MaxConcurrentCalls cannot be less than 1");
                }

                _maxConcurrentCalls = value;
                _callsCountToStartServerAfterSuspend = (ushort)((value / 3) * 2);
            }
        }

        /// <summary>
        /// Max length of request source data in bytes
        /// </summary>
        public long MaxRequestLength
        {
            get => _maxMessageLength;
            set
            {
                if (value < 1)
                {
                    throw new InvalidOperationException("MaxMessageLength cannot be less than 1");
                }

                _maxMessageLength = value;
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
        public RequestIdChecker RequestChecker
        {
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
        /// Get is server started now
        /// </summary>
        public bool Started { get; private set; } = false;

        /// <summary>
        /// Raises when need to write log
        /// </summary>
        public event EventHandler<LogRecordEventArgs> LogRecord;

        /// <summary>
        /// Raises before server start
        /// </summary>
        public event EventHandler<EventArgs> BeforeServerStart;

        /// <summary>
        /// Raises after server started
        /// </summary>
        public event EventHandler<EventArgs> AfterServerStarted;

        /// <summary>
        /// Raises after server stopped
        /// </summary>
        public event EventHandler<EventArgs> AfterServerStopped;

        /// <summary>
        /// Access to http listener before server starts listen
        /// </summary>
        public event EventHandler<TaggedEventArgs<HttpListener>> HttpListenerCustomSetting;

        /// <summary>
        /// Access to http context on client connect
        /// </summary>
        public event EventHandler<TaggedEventArgs<HttpListenerContext>> HttpRequestCustomHandling;

        /// <summary>
        /// Access to request on client connect
        /// </summary>
        public event EventHandler<TaggedEventArgs<Request>> UserRequest;

        /// <summary>
        /// Access to response on user request
        /// </summary>
        public event EventHandler<TaggedEventArgs<Response>> ServerResponse;

        /// <summary>
        /// Access to user token request
        /// </summary>
        public event EventHandler<TaggedEventArgs<UserTokenRequest>> UserTokenRequest;

        /// <summary>
        /// Access to user token response
        /// </summary>
        public event EventHandler<TaggedEventArgs<UserTokenResponse>> ServerUserTokenResponse;

        /// <summary>
        /// Access to error response
        /// </summary>
        public event EventHandler<TaggedEventArgs<ErrorResponse>> ErrorServerResponse;

        /// <summary>
        /// Raises before method calls
        /// </summary>
        public event EventHandler<RequestEventArgs> MethodCall;

        private HttpListener _listener;
        private bool _startedInternal = false;
        private ushort _connectionsCount = 0;
        private MethodsCaller<T> _caller = new MethodsCaller<T>();
        private ushort _maxConcurrentCalls = 20;
        private ushort _callsCountToStartServerAfterSuspend = 12;
        private long _maxMessageLength = 20000;
        private IAuthenticationValidator _authenticationValidator = new AuthenticationValidatorStub();
        private ITokenDistributor _tokenDistributor = new StandardTokenDistributor();
        private IBruteforceChecker _bruteforceCheckerByLogin = new StandardBruteforceChecker();
        private IBruteforceChecker _bruteforceCheckerByIpAddress = new StandardBruteforceChecker();
        private RequestIdChecker _requestChecker = new RequestIdChecker();
        private readonly object _stopOrStartServerLockerOnHandle = new object();
        private readonly TaskQueue _taskQueue;

        /// <summary>
        /// Start http server asynchronously
        /// </summary>
        public void StartAsync()
        {
            BeforeServerStart?.Invoke(this, EventArgs.Empty);

            _listener = new HttpListener();
            _listener.Prefixes.Add($"{(Ssl ? "https" : "http")}://+:{Port}/");
            _listener.Prefixes.Add($"{(Ssl ? "https" : "http")}://localhost:{Port}/");
            _listener.Prefixes.Add($"{(Ssl ? "https" : "http")}://127.0.0.1:{Port}/");

            HttpListenerCustomSetting?.Invoke(this, new TaggedEventArgs<HttpListener>(_listener));

            StartAsyncInternal();
        }

        private void StartAsyncInternal()
        {
            new Task(() =>
            {
                bool startError = false;
                if (!_listener.IsListening)
                {
                    try
                    {
                        _listener.Start();
                    }
                    catch (Exception e)
                    {
                        startError = true;
                        RaiseLogRecord(LogType.Error, e);
                    }
                }

                if (!startError)
                {
                    if (!Started)
                    {
                        Started = true;
                        AfterServerStarted?.Invoke(this, EventArgs.Empty);
                        RaiseLogRecord(LogType.Info, $"Server {(Ssl ? "https" : "http")}://localhost:{Port} started...");
                    }

                    _startedInternal = true;
                    while (Started && _startedInternal)
                    {
                        HttpListenerContext context = null;
                        try
                        {
                            context = _listener.GetContext();
                        }
                        catch (Exception e)
                        {
                            RaiseLogRecord(LogType.Error, e);
                            Stop();
                        }
                        if (context != null)
                        {
                            _taskQueue.Enqueue(() => HandleContext(context));
                        }
                    }
                    RaiseLogRecord(LogType.Info, "Server stopped or suspended...");
                }
            },
            TaskCreationOptions.LongRunning)
            .Start();
        }

        public void Stop()
        {
            if (Started)
            {
                Started = false;
                _listener.Close();
                AfterServerStopped?.Invoke(this, EventArgs.Empty);
                StopInternal();
            }
        }

        private void StopInternal() => _startedInternal = false;

        private void HandleContext(HttpListenerContext context)
        {
            HandleConnectionBegin();

            try
            {
                HttpRequestCustomHandling?.Invoke(this, new TaggedEventArgs<HttpListenerContext>(context));

                var clientIp = context.Request.RemoteEndPoint.Address.ToString();

                RaiseLogRecord(LogType.Debug, $"Client [{clientIp}] connected...");

                // Check is wait list contains current client IP
                if (BruteforceCheckerByIpAddress.IsWaitListContains(clientIp))
                {
                    throw new RemoteException(ErrorCode.BruteforceSuspicion, "by IP", clientIp);
                }

                if (context.Request.ContentLength64 > MaxRequestLength)
                {
                    throw new RemoteException(ErrorCode.TooMuchData, $"Data length cannot be more than {MaxRequestLength} bytes");
                }

                var content = new byte[context.Request.ContentLength64];
                context.Request.InputStream.Read(content, 0, content.Length);

                // If encrypted data is Request
                if (Encrypted<Request>.IsClass(content))
                {
                    var request = new Encrypted<Request>(content).Decrypt(SecretCode);
                    HandleRequest(request, context);
                }
                // If ecnrypted data is UserTokenRequest
                else if (Encrypted<UserTokenRequest>.IsClass(content))
                {
                    var request = new Encrypted<UserTokenRequest>(content).Decrypt(SecretCode);
                    HandleUserTokenRequest(request, context);
                }
                else
                {
                    throw new RemoteException(ErrorCode.UnknownData, clientIp);
                }
            }
            catch (RemoteException remoteException)
            {
                RaiseLogRecord(LogType.Info, remoteException);
                SendErrorResponse(remoteException.Data, context);
            }
            catch (Exception exception)
            {
                RaiseLogRecord(LogType.Error, exception);
                SendErrorResponse(new RemoteExceptionData(ErrorCode.InternalServerError, exception.Message), context);
            }
            finally
            {
                HandleConnectionEnd();
            }
        }

        private void HandleRequest(Request request, HttpListenerContext context)
        {
            UserRequest?.Invoke(this, new TaggedEventArgs<Request>(request));

            var clientIp = context.Request.RemoteEndPoint.Address.ToString();

            // Handle request id fabrication
            if (request.RequestId != request.RequestIdRepeat ||
                !RequestChecker.IsNewRequest(request.RequestId))
            {
                throw new RemoteException(ErrorCode.RequestIdFabrication);
            }

            RaiseLogRecord(LogType.Debug, $"Client [{clientIp}] request id [{request.RequestId}] normal...");

            // Authentication by token
            if (TokenDistributor.Authenticate(request.UserToken, out TokenInfo tokenInfo))
            {
                _currentRequestContext = new RequestContext(request, tokenInfo.UserName, clientIp);
            }
            else
            {
                throw new RemoteException(ErrorCode.UserTokenExpired, tokenInfo?.UserName ?? "/", clientIp);
            }

            RaiseLogRecord(LogType.Debug, $"User [{tokenInfo.UserName}][{clientIp}] connected...");

            if (MethodCall != null)
            {
                var beforeMethodCallEventArgs = new RequestEventArgs(request, clientIp, tokenInfo.UserName, tokenInfo.Token);
                MethodCall(this, beforeMethodCallEventArgs);
                if (beforeMethodCallEventArgs.ProhibitMethodExecution)
                {
                    throw new RemoteException(ErrorCode.AccessDenied, $"Access to method [{request.Method}] denied for user [{tokenInfo.UserName}][{clientIp}]");
                }
            }

            // Try to get method info and call
            var callInfo = _caller.Call(
                Methods,
                request.Method,
                request.Parameters,
                request.ReturnTypeName);

            // Check if method not exist
            if (callInfo.MethodNotFound)
            {
                throw new RemoteException(ErrorCode.MethodNotFound, tokenInfo.UserName, clientIp);
            }

            // Check parameters conflict
            if (callInfo.MoreThanOneMethodFound)
            {
                throw new RemoteException(ErrorCode.MoreThanOneMethodFound, tokenInfo.UserName, clientIp);
            }

            // If target method threw an exception
            if (callInfo.CallException != null)
            {
                throw new RemoteException(ErrorCode.InternalServerError, tokenInfo.UserName, clientIp, callInfo.CallException);
            }

            if (callInfo.IsArray)
            {
                SendResponse(callInfo.ResultArray, request.Method, context);
            }
            else
            {
                SendResponse(callInfo.Result, request.Method, context);
            }

            RaiseLogRecord(LogType.Debug, $"User [{tokenInfo.UserName}][{clientIp}] executed method [{request.Method}]...");
        }

        private void HandleUserTokenRequest(UserTokenRequest request, HttpListenerContext context)
        {
            UserTokenRequest?.Invoke(this, new TaggedEventArgs<UserTokenRequest>(request));

            var clientIp = context.Request.RemoteEndPoint.Address.ToString();

            // Check is wait list contains current client login
            if (BruteforceCheckerByLogin.IsWaitListContains(clientIp))
            {
                throw new RemoteException(ErrorCode.BruteforceSuspicion, request.Login, clientIp);
            }

            // Handle request id fabrication
            if (request.RequestId != request.RequestIdRepeat ||
                !RequestChecker.IsNewRequest(request.RequestId))
            {
                throw new RemoteException(ErrorCode.RequestIdFabrication);
            }

            RaiseLogRecord(LogType.Debug, $"Ip [{clientIp}] request id [{request.RequestId}] normal (user token request)...");

            // Authentication by login/password
            if (!AuthenticationValidator.Authenticate(request.Login, request.Password))
            {
                // Handle bruteforce suspicion by ip
                if (BruteforceCheckerByIpAddress.CheckIsBruteforce(clientIp))
                {
                    throw new RemoteException(ErrorCode.BruteforceSuspicion, "by IP", clientIp);
                }

                // Handle bruteforce suspicion by login
                if (BruteforceCheckerByLogin.CheckIsBruteforce(request.Login))
                {
                    throw new RemoteException(ErrorCode.BruteforceSuspicion, request.Login, clientIp);
                }

                throw new RemoteException(ErrorCode.LoginOrPasswordInvalid, clientIp);
            }

            RaiseLogRecord(LogType.Debug, $"User [{request.Login}][{clientIp}] password authentication success...");

            var newToken = TokenDistributor.RequestToken(request.Login, clientIp);

            SendUserTokenResponse(newToken, context);

            RaiseLogRecord(LogType.Info, $"Server issued new token [{newToken}] for user [{request.Login}][{clientIp}]...");
        }

        private void SendResponse(object result, string method, HttpListenerContext context)
        {
            SendResponse(
                new Response() { Result = result, Method = method, ServerTime = DateTime.Now },
                context);
        }

        private void SendResponse(Array resultArr, string method, HttpListenerContext context)
        {
            SendResponse(
                new Response()
                {
                    ResultArray = resultArr as object[],
                    IsEmptyArray = resultArr != null && resultArr.Length == 0 ? (bool?)true : null,
                    Method = method,
                    ServerTime = DateTime.Now
                },
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
            ErrorServerResponse?.Invoke(this, new TaggedEventArgs<ErrorResponse>(response));
            try
            {
                if (response.ErrorData.Code == ErrorCode.UnknownData)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Close();
                }
                if (response.ErrorData.Code == ErrorCode.DecryptionErrorCode)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unused;
                    context.Response.Close();
                }
                else
                {
                    var encryptedResponse = new Encrypted<ErrorResponse>(response, SecretCode);
                    var bytes = encryptedResponse.Data;
                    context.Response.ContentLength64 = bytes.Length;
                    context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    context.Response.OutputStream.Close();
                    context.Response.Close();
                }
            }
            catch (Exception e)
            {
                RaiseLogRecord(LogType.Error, e);
            }
        }

        private void SendResponse(Response response, HttpListenerContext context)
        {
            ServerResponse?.Invoke(this, new TaggedEventArgs<Response>(response));
            var encryptedResponse = new Encrypted<Response>(response, SecretCode);
            var bytes = encryptedResponse.Data;
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
            context.Response.Close();
        }

        private void SendUserTokenResponse(UserTokenResponse tokenResponse, HttpListenerContext context)
        {
            ServerUserTokenResponse?.Invoke(this, new TaggedEventArgs<UserTokenResponse>(tokenResponse));
            var encryptedResponse = new Encrypted<UserTokenResponse>(tokenResponse, SecretCode);
            var bytes = encryptedResponse.Data;
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
            context.Response.Close();
        }

        private void HandleConnectionBegin()
        {
            lock (_stopOrStartServerLockerOnHandle)
            {
                _connectionsCount++;
                if (_connectionsCount >= MaxConcurrentCalls && _startedInternal)
                {
                    RaiseLogRecord(LogType.Info, $" ** Too many connections ({_connectionsCount}). Server suspend.");
                    StopInternal();
                }
            }
        }

        private void HandleConnectionEnd()
        {
            lock (_stopOrStartServerLockerOnHandle)
            {
                _connectionsCount--;
                if (_connectionsCount <= _callsCountToStartServerAfterSuspend && !_startedInternal && Started)
                {
                    RaiseLogRecord(LogType.Info, " ** Connections count normal. Server continues to work.");
                    StartAsyncInternal();
                }
            }
        }

        private void RaiseLogRecord(LogType type, Exception exception) => LogRecord?.Invoke(this, new LogRecordEventArgs(type, exception));

        private void RaiseLogRecord(LogType type, string message) => LogRecord?.Invoke(this, new LogRecordEventArgs(type, message));

        public void Dispose()
        {
            if (Started)
            {
                Stop();
            }
        }
    }
}