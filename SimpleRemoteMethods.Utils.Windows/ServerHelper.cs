using SimpleRemoteMethods.ServerSide;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SimpleRemoteMethods.Utils.Windows
{
    /// <summary>
    /// Preparations for start http or https server in windows
    /// </summary>
    public static class ServerHelper
    {
        /// <summary>
        /// Log event caller
        /// </summary>
        public static EventHandler<LogRecordEventArgs> LogRecord { get; set; }

        private static void RaiseLog<T>(Server<T> server, string message)
        {
            LogRecord?.Invoke(server, new LogRecordEventArgs(LogType.Info, message));
#if DEBUG
            Console.WriteLine(message);
#endif
        }

        /// <summary>
        /// Prepare windows for current HTTP server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="server"></param>
        /// <param name="appId">Unique identificator of current executable (need for naming of security settings in windows firewall, etc)</param>
        public static void PrepareHttpServer<T>(Server<T> server, string appId)
        {
            var serverAddress = $"http://localhost:{server.Port}";
            var firewallRuleName = (typeof(T).Name).Replace("'", string.Empty) + $"_{appId}_http";

            void log(string message) => RaiseLog(server, message);

            server.BeforeServerStart += (o, e) =>
            {
                log($"Prepare [{serverAddress}] for Windows");
                SecurityHelper.ReserveUrl(serverAddress, log);
                SecurityHelper.AddFirewallRuleForPort(firewallRuleName, server.Port, log);
            };

            server.AfterServerStopped += (o, e) =>
            {
                log($"Remove preparation [{serverAddress}] from Windows");
                SecurityHelper.RemoveAddressReservation(serverAddress, log);
                SecurityHelper.RemoveFirewallRule(firewallRuleName, log);
            };
        }

        /// <summary>
        /// Prepare windows for current HTTPS server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="server"></param>
        /// <param name="certificateHash">Certificate hash from windows cetificates store</param>
        /// <param name="appId">Unique identificator of current executable (need for naming of security settings in windows firewall, etc)</param>
        public static void PrepareHttpsServer<T>(Server<T> server, string certificateHash, string appId)
        {
            var serverAddress = $"https://localhost:{server.Port}";
            var firewallRuleName = (typeof(T).Name).Replace("'", string.Empty) + $"_{appId}_https";

            void log(string message) => RaiseLog(server, message);

            server.BeforeServerStart += (o, e) =>
            {
                log($"Prepare [{serverAddress}] for Windows");
                SecurityHelper.BindCertificateToPort(certificateHash, server.Port, log);
                SecurityHelper.ReserveUrl(serverAddress, log);
                SecurityHelper.AddFirewallRuleForPort(firewallRuleName, server.Port, log);
            };

            server.AfterServerStopped += (o, e) =>
            {
                log($"Remove preparation [{serverAddress}] from Windows");
                SecurityHelper.RemoveAddressReservation(serverAddress, log);
                SecurityHelper.RemoveFirewallRule(firewallRuleName, log);
                SecurityHelper.UnbindCertificatesFromPort(server.Port, log);
            };
        }

        /// <summary>
        /// Get installed certificates
        /// </summary>
        /// <returns></returns>
        public static CertificateInfo[] GetInstalledCertificates()
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            return store
                .Certificates
                .Cast<X509Certificate2>().Select(x => new CertificateInfo()
                {
                    Hash = x.GetCertHashString(),
                    Description = x.IssuerName.Name + " / " + x.SubjectName.Name
                }).ToArray();
        }

        public class CertificateInfo
        {
            public string Description { get; internal set; }
            public string Hash { get; internal set; }
        }
    }
}