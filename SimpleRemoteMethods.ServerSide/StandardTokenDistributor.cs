using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    public class StandardTokenDistributor : ITokenDistributor
    {
        private Dictionary<string, TokenInfo> _tokens = new Dictionary<string, TokenInfo>();

        public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(10);

        public bool Authenticate(string token, out TokenInfo tokenInfo)
        {
            lock (_tokens)
                ClearOutdatedTokens();

            if (_tokens.ContainsKey(token))
                tokenInfo = _tokens[token];
            else tokenInfo = null;

            return tokenInfo != null;
        }

        public string RequestToken(string userName, string clientIp)
        {
            var existingToken = GetTokenInfo(userName, clientIp);
            if (existingToken != null)
                _tokens.Remove(existingToken.Token);

            existingToken = new TokenInfo();
            existingToken.ClientIp = clientIp;
            existingToken.UserName = userName;
            existingToken.Token = CreateNewTokenString();
            existingToken.DistributionDate = DateTime.Now;
            _tokens.Add(existingToken.Token, existingToken);
            return existingToken.Token;
        }

        public void RevokeToken(string userName)
        {
            lock (_tokens)
                foreach (var tokenInfo in _tokens.Values)
                    if (tokenInfo.UserName == userName)
                        _tokens.Remove(tokenInfo.Token);
        }

        private string CreateNewTokenString()
        {
            var guid = Guid.NewGuid().ToString();
            while (_tokens.ContainsKey(guid))
                guid = Guid.NewGuid().ToString();
            return guid;
        }

        private TokenInfo GetTokenInfo(string userName, string clientIp)
        {
            var tokenInfo = _tokens.Values
                .FirstOrDefault(x => x.ClientIp == clientIp && x.UserName == userName);

            return tokenInfo;
        }

        private void ClearOutdatedTokens()
        {
            foreach (var tokenInfo in _tokens.Values)
                if (tokenInfo.DistributionDate < (DateTime.Now - TokenLifetime))
                    _tokens.Remove(tokenInfo.Token);
        }
    }
}
