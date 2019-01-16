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
        /// Method input parameters
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// Name of object class that method returns
        /// </summary>
        public string ReturnTypeName { get; set; }

        /// <summary>
        /// Authentication user token
        /// </summary>
        public string UserToken { get; set; }

        /// Intruder can change request id even if it encrypted by changing encrypted bytes to random
        /// in place where RequestId parameter setted; repeat of request id and checking it on server side
        /// reduces to nothing intruder attemts to change request id.
        /// <summary>
        /// Unique requst id repeat
        /// </summary>
        public string RequestIdRepeat { get; set; }
    }
}
