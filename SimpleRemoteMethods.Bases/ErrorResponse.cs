using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Class for transfer error from server to client
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Object that contains information about server error
        /// </summary>
        public RemoteExceptionData ErrorData { get; set; }
    }
}
