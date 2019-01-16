using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    public class StandardBruteforceChecker: IBruteforceChecker
    {
        public static ushort LoginHoursWaitTime = 2;
        public static ushort LoginTryLifetimeMinutes = 10;
        public static ushort LoginTryCount = 5;

        private Dictionary<string, LoginInfo> _loginInfos = new Dictionary<string, LoginInfo>();

        private LoginInfo PrepareLastTryObject(string loginString)
        {
            lock (_loginInfos)
            {
                if (_loginInfos.ContainsKey(loginString))
                    return _loginInfos[loginString];
                else
                {
                    var info = new LoginInfo();
                    _loginInfos.Add(loginString, info);
                    return info;
                };
            }
        }

        private bool CheckBruteforceInternal(LoginInfo info)
        {
            lock (info)
            {
                var timePassed = DateTime.Now - info.LastLoginTry;
                if ((!info.IsBrutforceSuspicion && timePassed.TotalMinutes > LoginTryLifetimeMinutes) ||
                    (info.IsBrutforceSuspicion && timePassed.TotalHours > LoginHoursWaitTime))
                {
                    info.TryCount = 0;
                    info.IsBrutforceSuspicion = false;
                }
                else if (!info.IsBrutforceSuspicion && timePassed.TotalMinutes < LoginTryLifetimeMinutes && info.TryCount >= LoginTryCount)
                {
                    info.IsBrutforceSuspicion = true;
                }
                else if (!info.IsBrutforceSuspicion && timePassed.TotalMinutes < LoginTryLifetimeMinutes && info.TryCount < LoginTryCount)
                {
                    info.TryCount++;
                }
                info.LastLoginTry = DateTime.Now;
                return info.IsBrutforceSuspicion;
            }
        }

        public bool CheckIsBruteforce(string loginString)
        {
            var info = PrepareLastTryObject(loginString);
            return CheckBruteforceInternal(info);
        }

        public bool IsWaitListContains(string loginString)
        {
            var info = PrepareLastTryObject(loginString);
            return info.IsBrutforceSuspicion && (DateTime.Now - info.LastLoginTry).TotalHours < LoginHoursWaitTime;
        }

        private class LoginInfo
        {
            public DateTime LastLoginTry { get; set; } = DateTime.MinValue;
            public int TryCount { get; set; } = 1;
            public bool IsBrutforceSuspicion { get; set; }
        }
    }
}
