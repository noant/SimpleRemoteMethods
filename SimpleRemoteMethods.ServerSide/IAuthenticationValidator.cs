using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    public interface IAuthenticationValidator
    {
        bool Authenticate(string userName, string password);
    }
}
