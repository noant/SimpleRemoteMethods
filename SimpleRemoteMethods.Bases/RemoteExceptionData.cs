using ProtoBuf;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Exception or communication error of server or client
    /// </summary>
    [ProtoContract]
    public sealed class RemoteExceptionData
    {
        /// <summary>
        /// Create new data
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="message">Custom message</param>
        public RemoteExceptionData(ErrorCode code, string message = "")
        {
            Code = code;
            Message = message;
        }

        public RemoteExceptionData() { }

        /// <summary>
        /// Custom message of error
        /// </summary>
        [ProtoMember(1)]
        public string Message { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public ErrorCode Code { get; set; }
    }
}
