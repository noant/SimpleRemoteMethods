using ProtoBuf;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Request for server to get user token
    /// </summary>
    [ProtoContract]
    public class UserTokenRequest
    {
        /// <summary>
        /// Unique id of request
        /// </summary>
        [ProtoMember(1)]
        public string RequestId { get; set; }

        /// <summary>
        /// User login
        /// </summary>
        [ProtoMember(2)]
        public string Login { get; set; }

        /// <summary>
        /// User password
        /// </summary>
        [ProtoMember(3)]
        public string Password { get; set; }

        /// Intruder can change request id even if it encrypted by changing encrypted bytes to random
        /// in place where RequestId parameter setted; repeat of request id and checking it on server side
        /// reduces to nothing intruder attemts to change request id.
        /// <summary>
        /// Unique id of request
        /// </summary>
        [ProtoMember(4)]
        public string RequestIdRepeat { get; set; }
    }
}
