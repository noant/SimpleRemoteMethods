using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    public class Response
    {
        public string Method { get; set; }
        public object Result { get; set; }
        public RemoteExceptionData RemoteException { get; set; }
        public DateTime ServerTime { get; set; }
    }
}
