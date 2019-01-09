using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    public class Request
    {
        public string RequestId { get; set; }
        public string Method { get; set; }
        public object[] Parameters { get; set; }
        public string UserToken { get; set; }
    }
}
