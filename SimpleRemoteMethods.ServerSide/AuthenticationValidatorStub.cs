using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    public class AuthenticationValidatorStub : IAuthenticationValidator
    {
        public bool Authenticate(string userName, string password)
        {
            return true;
        }
    }
}
