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
        /// Object that contains methods for remote use
        /// </summary>
        public T Methods { get; }

        /// <summary>
        /// User/password validator
        /// </summary>
        public IAuthenticationValidator AuthenticationValidator = new AuthenticationValidatorStub();

        /// <summary>
        /// Logic for user token destribution
        /// </summary>
        public ITokenDistributor TokenDistributor = new StandardTokenDistributor();

        /// <summary>
        /// Bruteforce checker by login
        /// </summary>
        public IBruteforceChecker BruteforceCheckerByLogin = new StandardBruteforceChecker();

        /// <summary>
        /// Bruteforce checker by ip
        /// </summary>
        public IBruteforceChecker BruteforceCheckerByIpAddress = new StandardBruteforceChecker();

        /// <summary>
        /// Object for tracking and checking request id
        /// </summary>
        public RequestIdChecker RequestChecker = new RequestIdChecker();

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

        /// <summary>
        /// Start http server asynchronously
        /// </summary>
        public async void StartAsync()
        {
            await new Task(() => {
                _listener = new HttpListener();
                _listener.Prefixes.Add(string.Format("{0}://+:{1}/", UseHttps ? "https" : "http", Port));
                _listener.Start();
                while (true)
                {
                    var context = _listener.GetContext();
                    Task.Run(() => HandleContext(context));
                }
            }, 
            TaskCreationOptions.LongRunning);
        }

        private void HandleContext(HttpListenerContext context)
        {
            var clientIp = context.Request.RemoteEndPoint.ToString();

            if (BruteforceCheckerByIpAddress.CheckIsBruteforce(clientIp))
                throw RemoteException.Get(RemoteExceptionData.BruteforceSuspicion, "/", clientIp);

            var buff = new byte[context.Request.InputStream.Length];
            context.Request.InputStream.Read(buff, 0, buff.Length);
            var sourceStr = Encoding.UTF8.GetString(buff);
            if (Encrypted<Request>.IsClass(sourceStr))
            {
                var request = Encrypted<Request>.FromString(sourceStr).Decrypt(SecretCode);

                if (request.RequestId != request.RequestIdRepeat ||
                    !RequestChecker.IsNewRequest(request.RequestId))
                    throw RemoteException.Get(RemoteExceptionData.RequestIdFabrication);

                if (TokenDistributor.Authenticate(request.UserToken, out TokenInfo tokenInfo))
                    _currentRequestContext = new RequestContext(request, tokenInfo.UserName, clientIp);
                else
                    throw RemoteException.Get(RemoteExceptionData.UserTokenExpired, tokenInfo?.UserName ?? "/", clientIp);
            }
            else if (Encrypted<UserTokenRequest>.IsClass(sourceStr))
            {

            }
            else
                throw RemoteException.Get(RemoteExceptionData.UnknownData, clientIp);
        }
    }
}
