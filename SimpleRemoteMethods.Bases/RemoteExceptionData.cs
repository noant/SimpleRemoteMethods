using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Exception or communication error of server or client
    /// </summary>
    public sealed class RemoteExceptionData
    {
        /// <summary>
        /// Create new data
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="message">Custom message</param>
        public RemoteExceptionData(string code, string message = "")
        {
            Code = code;
            Message = message;
        }

        public RemoteExceptionData() { }

        /// <summary>
        /// Custom message of error
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        public string Code { get; set; }

        public const string DecryptionErrorCode = "0";
        public const string UnknownData = "1";
        public const string UserTokenExpired = "2";
        public const string LoginOrPasswordInvalid = "4";
    }
}
