using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    public class StandardTokenDistributor : ITokenDistributor
    {
        private Dictionary<string, TokenInfo> _tokens = new Dictionary<string, TokenInfo>();

        public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(10);

        public bool Authenticate(string token, out string userName)
        {
            throw new NotImplementedException();
        }

        public string RequestToken(string userName, string clientIp)
        {
            throw new NotImplementedException();
        }

        private class TokenInfo
        {
            public string Token { get; set; }
            public string UserName { get; set; }
            public string ClientIp { get; set; }
            public DateTime DistributionDate { get; set; }
        }
    }
}
