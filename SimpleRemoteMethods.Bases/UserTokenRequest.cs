using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Request for server to get user token
    /// </summary>
    public class UserTokenRequest
    {
        /// <summary>
        /// Unique id of request
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// User login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// User password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Unique id of request
        /// </summary>
        public string RequestIdRepeat { get; set; }
    }
}
