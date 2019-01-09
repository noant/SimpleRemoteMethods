using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    public interface ITokenDistributor
    {
        bool Authenticate(string token, out string userName);
        string RequestToken(string userName, string clientIp);
        TimeSpan TokenLifetime { get; set; }
    }
}
