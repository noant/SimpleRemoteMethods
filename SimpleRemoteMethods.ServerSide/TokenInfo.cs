using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    /// <summary>
    /// Information about user ip and token
    /// </summary>
    public class TokenInfo
    {
        /// <summary>
        /// Unique id of user/ip
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// User login
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Ip address of client
        /// </summary>
        public string ClientIp { get; set; }

        /// <summary>
        /// Date the token was created
        /// </summary>
        public DateTime DistributionDate { get; set; }
    }
}
