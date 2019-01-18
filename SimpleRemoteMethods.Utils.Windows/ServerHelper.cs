using SimpleRemoteMethods.ServerSide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.Utils.Windows
{
    /// <summary>
    /// Preparations for start http or https server in windows
    /// </summary>
    public static class ServerHelper
    {
        /// <summary>
        /// Prepare windows for current HTTP server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="server"></param>
        public static void PrepareHttpServer<T>(Server<T> server)
        {
            var serverAddress = string.Format("http://localhost:{0}", server.Port);
            var currentServerName = (typeof(T).Name).Replace("'", string.Empty) + "-http";

            server.BeforeServerStart += (o, e) => {
                SecurityHelper.ReserveUrl(serverAddress);
                SecurityHelper.AddFirewallRuleForPort(currentServerName, server.Port);
            };

            server.AfterServerStopped += (o, e) => {
                SecurityHelper.RemoveAddressReservation(serverAddress);
                SecurityHelper.RemoveFirewallRule(currentServerName);
            };
        }

        /// <summary>
        /// Prepare windows for current HTTPS server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="server"></param>
        /// <param name="certificateHash">Certificate hash from windows cetificates store</param>
        public static void PrepareHttpsServer<T>(Server<T> server, string certificateHash)
        {
            var serverAddress = string.Format("https://localhost:{0}", server.Port);
            var currentServerName = (typeof(T).Name).Replace("'", string.Empty) + "-https";
            
            server.BeforeServerStart += (o, e) => {
                SecurityHelper.BindCertificateToPort(certificateHash, server.Port);
                SecurityHelper.ReserveUrl(serverAddress);
                SecurityHelper.AddFirewallRuleForPort(currentServerName, server.Port);
            };

            server.AfterServerStopped += (o, e) => {
                SecurityHelper.RemoveAddressReservation(serverAddress);
                SecurityHelper.RemoveFirewallRule(currentServerName);
                SecurityHelper.UnbindCertificatesFromPort(server.Port);
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
