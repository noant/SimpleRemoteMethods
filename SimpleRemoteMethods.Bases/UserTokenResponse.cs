using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Response from server with user token or error state
    /// </summary>
    [ProtoContract]
    public class UserTokenResponse
    {
        /// <summary>
        /// New toke for user
        /// </summary>
        [ProtoMember(1)]
        public string UserToken { get; set; }
    }
}
