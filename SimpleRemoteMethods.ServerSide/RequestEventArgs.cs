using SimpleRemoteMethods.Bases;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    /// <summary>
    /// Request event args
    /// </summary>
    public class RequestEventArgs: EventArgs
    {
        public RequestEventArgs(Request request, string clientIp, string userName, string userToken)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            ClientIp = clientIp ?? throw new ArgumentNullException(nameof(clientIp));
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            UserToken = userToken ?? throw new ArgumentNullException(nameof(userToken));
        }

        /// <summary>
        /// Request data
        /// </summary>
        public Request Request { get; }

        /// <summary>
        /// Client ip address
        /// </summary>
        public string ClientIp { get; }

        /// <summary>
        /// Client user name
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Client user token
        /// </summary>
        public string UserToken { get; }

        /// <summary>
        /// if set true, the server throws an exception to user "access denied"
        /// </summary>
        public bool ProhibitMethodExecution { get; set; }
    }
}
