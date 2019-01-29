using System;

namespace SimpleRemoteMethods.ServerSide
{
    /// <summary>
    /// Class that conains logic for user token distribution
    /// </summary>
    public interface ITokenDistributor
    {
        /// <summary>
        /// Returns true if the token was created and it is still alive
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokenInfo">Info about token (user name, etc)</param>
        /// <returns></returns>
        bool Authenticate(string token, out TokenInfo tokenInfo);

        /// <summary>
        /// Creates new token for user/ip
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="clientIp"></param>
        /// <returns>token string</returns>
        string RequestToken(string userName, string clientIp);

        /// <summary>
        /// Cancel user token
        /// </summary>
        /// <param name="userName"></param>
        void RevokeToken(string userName);

        /// <summary>
        /// The time interval while the token is alive
        /// </summary>
        TimeSpan TokenLifetime { get; set; }
    }
}
