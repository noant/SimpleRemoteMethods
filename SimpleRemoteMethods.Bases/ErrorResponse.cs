using ProtoBuf;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Class for transfer error from server to client
    /// </summary>
    [ProtoContract]
    public class ErrorResponse
    {
        /// <summary>
        /// Object that contains information about server error
        /// </summary>
        [ProtoMember(1)]
        public RemoteExceptionData ErrorData { get; set; }
    }
}
