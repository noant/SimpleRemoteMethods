using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Response from server with user token or error state
    /// </summary>
    public class UserTokenResponse
    {
        /// <summary>
        /// New toke for user
        /// </summary>
        public string UserToken { get; set; }

        /// <summary>
        /// Error from server
        /// </summary>
        public RemoteExceptionData RemoteException { get; set; }
    }
}
