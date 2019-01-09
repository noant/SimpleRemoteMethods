using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    public class UserTokenRequest
    {
        public string RequestId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
