using System;

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
        /// SSL mode
        /// </summary>
        public bool UseHttps { get; set; }

        /// <summary>
        /// Server port
        /// </summary>
        public ushort Port { get; set; }
    }
}
