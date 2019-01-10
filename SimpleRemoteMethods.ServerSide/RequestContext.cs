using SimpleRemoteMethods.Bases;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    /// <summary>
    /// Request context
    /// </summary>
    public class RequestContext
    {
        public RequestContext(Request request, string userName, string clientIp)
        {
            Request = request;
            UserName = userName;
            ClientIp = clientIp;
        }

        /// <summary>
        /// Request info
        /// </summary>
        public Request Request { get; }

        /// <summary>
        /// Caller user name
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Client ip address
        /// </summary>
        public string ClientIp { get; }
    }
}
