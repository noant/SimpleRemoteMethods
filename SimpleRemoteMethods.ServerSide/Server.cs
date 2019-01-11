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
        public Server(T objectMethods)
        {
            Methods = objectMethods;
        }

        /// <summary>
        /// Max synchronous connections
        /// </summary>
        public ushort MaxConcurrentCalls { get; set; } = 20;

        /// <summary>
        /// Object that contains methods for remote use
        /// </summary>
        public T Methods { get; }

        /// <summary>
        /// User/password validator
        /// </summary>
        public IAuthenticationValidator AuthenticationValidator { get; set; } = new AuthenticationValidatorStub();

        /// <summary>
        /// Logic for user token destribution
        /// </summary>
        public ITokenDistributor TokenDistributor { get; set; } = new StandardTokenDistributor();

        /// <summary>
        /// Bruteforce checker by login
        /// </summary>
        public IBruteforceChecker BruteforceCheckerByLogin { get; set; } = new StandardBruteforceChecker();

        /// <summary>
        /// Bruteforce checker by ip
        /// </summary>
        public IBruteforceChecker BruteforceCheckerByIpAddress { get; set; } = new StandardBruteforceChecker();

        /// <summary>
        /// Object for tracking and checking request id
        /// </summary>
        public RequestIdChecker RequestChecker { get; set; } = new RequestIdChecker();

        /// <summary>
        /// SSL mode
        /// </summary>
        public bool UseHttps { get; set; }

        /// <summary>
        /// Server port
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Secret code that encrypts all data
        /// </summary>
        public string SecretCode { get; set; }

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

        /// <summary>
        /// Start http server asynchronously
        /// </summary>
        public async void StartAsync()
        {
            _started = true;
            _listener = new HttpListener();
            _listener.Prefixes.Add(string.Format("{0}://+:{1}/", UseHttps ? "https" : "http", Port));
            await StartAsyncInternal();
        }

        private async Task StartAsyncInternal()
        {
            _startedInternal = true;
            await new Task(() => {
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

                    // Handle request id fabrication
                    if (request.RequestId != request.RequestIdRepeat ||
                        !RequestChecker.IsNewRequest(request.RequestId))
                        throw RemoteException.Get(RemoteExceptionData.RequestIdFabrication);

                    // Authentication by token
                    if (TokenDistributor.Authenticate(request.UserToken, out TokenInfo tokenInfo))
                        _currentRequestContext = new RequestContext(request, tokenInfo.UserName, clientIp);
                    else
                        throw RemoteException.Get(RemoteExceptionData.UserTokenExpired, tokenInfo?.UserName ?? "/", clientIp);
                    
                    // Try to get method info and call
                    var callInfo = _caller.Call(Methods, request.Method, request.Parameters, request.ReturnTypeName);

                    // Check if method not exist
                    if (callInfo.MethodNotFound)
                        throw RemoteException.Get(RemoteExceptionData.MethodNotFound, tokenInfo.UserName, clientIp);

                    // If target method threw an exception
                    if (callInfo.CallException != null)
                        throw RemoteException.Get(RemoteExceptionData.InternalServerError, tokenInfo.UserName, clientIp, callInfo.CallException);

                    SendResponse(callInfo.Result, request.Method, context);
                }
                // If ecnrypted data is UserTokenRequest
                else if (Encrypted<UserTokenRequest>.IsClass(sourceStr))
                {

                }
                else
                    throw RemoteException.Get(RemoteExceptionData.UnknownData, clientIp);
            }
            catch (RemoteException remoteException)
            {
                SendResponse(remoteException.Data, string.Empty, context);
            }
            catch (Exception exception)
            {
                SendResponse(new RemoteExceptionData(RemoteExceptionData.InternalServerError, exception.Message), string.Empty, context);
            }
            finally
            {
                HandleConnectionEnd();
            }
        }

        private void SendResponse(object result, string method, HttpListenerContext context)
        {
            SendResponse(
                new Response() { Result = result, Method = method, ServerTime = DateTime.Now },
                context);
        }
        
        private void SendResponse(RemoteExceptionData exceptionData, string method, HttpListenerContext context)
        {
            SendResponse(
                new Response() { RemoteException = exceptionData, Method = method, ServerTime = DateTime.Now },
                context);
        }

        private void SendResponse(Response response, HttpListenerContext context)
        {
            var encryptedResponse = new Encrypted<Response>(response, SecretCode);
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(encryptedResponse.ToString()));
            context.Response.OutputStream.Close();
        }

        private void HandleConnectionBegin()
        {
            _connectionsCount++;
            if (_connectionsCount >= MaxConcurrentCalls)
                StopInternal();
        }

        private async void HandleConnectionEnd()
        {
            _connectionsCount--;
            if (_connectionsCount < MaxConcurrentCalls && !_startedInternal && _started)
                await StartAsyncInternal();
        }
    }
}
