using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Exception that throws on server or client when request or response contains error data
    /// </summary>
    public sealed class RemoteException: Exception
    {
        public static RemoteException Get(string code, string message = "", Exception inner = null) => new RemoteException(new RemoteExceptionData(code, message), inner);
        public static RemoteException Get(string code, string user, string clientIp, Exception inner = null) => new RemoteException(new RemoteExceptionData(code, string.Format("User: {0}, {1}", user, clientIp)), inner);
        public static RemoteException Get(RemoteExceptionData data) => new RemoteException(data);

        /// <summary>
        /// Create new exception
        /// </summary>
        /// <param name="data">Exception data</param>
        /// <param name="innerException">Details</param>
        public RemoteException(RemoteExceptionData data, Exception innerException = null):
            base($"Error code: {data.Code}. {data.Message}. {innerException?.Message}", innerException)
        {
            Data = data;
            Code = data.Code;
        }

        /// <summary>
        /// Exception data
        /// </summary>
        public new RemoteExceptionData Data { get; }

        /// <summary>
        /// Error code (from RemoteExceptionData codes)
        /// </summary>
        public string Code { get; }
    }
}
