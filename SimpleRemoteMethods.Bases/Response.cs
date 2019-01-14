using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Response from server with custom data
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Method called on server
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Target data
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Server time
        /// </summary>
        public DateTime ServerTime { get; set; }
    }
}
