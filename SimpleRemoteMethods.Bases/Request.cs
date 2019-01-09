using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Request to server to get cusom data
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Unique requst id
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Method called on server
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Methof input parameters
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// Authentication user token
        /// </summary>
        public string UserToken { get; set; }
    }
}
