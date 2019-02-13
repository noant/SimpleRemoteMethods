using System;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Exception that throws on server or client when request or response contains error data
    /// </summary>
    public sealed class RemoteException: Exception
    {
        /// <summary>
        /// Create new exception
        /// </summary>
        /// <param name="data">Exception data</param>
        /// <param name="innerException">Details</param>
        public RemoteException(RemoteExceptionData data, Exception innerException = null):
            base($"{Utils.GetErrorCodeDescription(data.Code)}. {data.Message}. {innerException?.Message}", innerException)
        {
            Data = data;
            Code = data.Code;
        }

        /// <summary>
        /// Create new exception
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="message">Custom mesage</param>
        /// <param name="innerException">Inner exception</param>
        public RemoteException(ErrorCode code, string message = "", Exception innerException = null):
            this(new RemoteExceptionData(code, message), innerException)
        {
            // Empty
        }

        /// <summary>
        /// Create new exception
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="user">User name or login</param>
        /// <param name="clientIp">Client ip</param>
        /// <param name="innerException">Inner exception</param>
        public RemoteException(ErrorCode code, string user, string clientIp, Exception innerException = null):
            this(code, $"User: {user ?? "[unknown]"}, IP: {clientIp ?? "[unknown]"}", innerException)
        {
            // Empty
        }

        /// <summary>
        /// Exception data
        /// </summary>
        public new RemoteExceptionData Data { get; }

        /// <summary>
        /// Error code (from RemoteExceptionData codes)
        /// </summary>
        public ErrorCode Code { get; }
    }
}
