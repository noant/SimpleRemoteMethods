using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    public class UserTokenResponse
    {
        public string UserToken { get; set; }
        public RemoteExceptionData RemoteException { get; set; }
    }
}
