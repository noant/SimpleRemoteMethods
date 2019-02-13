using SslCertBinding.Net;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace SimpleRemoteMethods.Utils.Windows
{
    public static class SecurityHelper
    {
        /// <summary>
        /// Bind certificate (installed in windows) to port
        /// </summary>
        /// <param name="certificateHash"></param>
        /// <param name="port"></param>
        public static void BindCertificateToPort(string certificateHash, ushort port)
        {
            UnbindCertificatesFromPort(port);
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var cert = store
                .Certificates
                .Cast<X509Certificate2>()
                .FirstOrDefault(x => x.GetCertHashString().Equals(certificateHash));

            if (cert == null)
                throw new Exception($"Cannot found certificate [{certificateHash}]");

            var appid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;

            var certificateBindingConfiguration = new CertificateBindingConfiguration();

            certificateBindingConfiguration.Bind(
                new CertificateBinding(
                    certificateHash,
                    StoreName.My,
                    new IPEndPoint(new IPAddress(new byte[] { 0, 0, 0, 0 }), port),
                    Guid.Parse(appid))
            );
        }

        /// <summary>
        /// Unbind certificate from port
        /// </summary>
        /// <param name="port"></param>
        public static void UnbindCertificatesFromPort(ushort port)
        {
            var command = " http delete sslcert ipport=0.0.0.0:" + port;
            ExecuteProcess(Path.Combine(Environment.SystemDirectory, "netsh.exe"), command);
        }

        /// <summary>
        /// Reserve url in windows
        /// </summary>
        /// <param name="address"></param>
        public static void ReserveUrl(string address)
        {
            RemoveAddressReservation(address);
            var commandString = $@" http add urlacl url={address} user={Environment.UserDomainName}\{Environment.UserName}";

            ExecuteProcess(Path.Combine(Environment.SystemDirectory, "netsh.exe"), commandString);
        }

        /// <summary>
        /// Remove url reservation from windows
        /// </summary>
        /// <param name="address"></param>
        public static void RemoveAddressReservation(string address)
        {
            var commandString = $@" http delete urlacl url={address.Replace("https://", "http://")}";
            ExecuteProcess(Path.Combine(Environment.SystemDirectory, "netsh.exe"), commandString);
        }

        /// <summary>
        /// Add new certificate in windows
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string AddCertificateInWindows(string filename, string password)
        {
            var certificate = new X509Certificate2(filename, password);
            var name = certificate.Subject.Replace("CN=", "");
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);

            var cert = new X509Certificate2(certificate);
            store.Add(cert);
            store.Close();
            return cert.GetCertHashString();
        }

        /// <summary>
        /// Create firewall rule for port
        /// </summary>
        /// <param name="port"></param>
        public static void AddFirewallRuleForPort(string ruleName, ushort port)
        {
            var commandRemove = $" advfirewall firewall delete rule name = \"{ruleName}\"";
            var commandAdd = $" firewall add portopening TCP {port} {ruleName} enable ALL";
            var netshpath = Path.Combine(Environment.SystemDirectory, "netsh.exe");
            ExecuteProcess(netshpath, commandRemove);
            ExecuteProcess(netshpath, commandAdd);
        }

        /// <summary>
        /// Remove firewall rule
        /// </summary>
        /// <param name="ruleName"></param>
        public static void RemoveFirewallRule(string ruleName)
        {
            var commandRemove = $" advfirewall firewall delete rule name = \"{ruleName}\"";
            var netshpath = Path.Combine(Environment.SystemDirectory, "netsh.exe");
            ExecuteProcess(netshpath, commandRemove);
        }

        /// <summary>
        /// Execute process
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="arguments"></param>
        /// <param name="asAdmin"></param>
        /// <param name="waitForExit"></param>
        /// <param name="priority"></param>
        public static void ExecuteProcess(string filePath, string arguments, bool asAdmin = false, bool waitForExit = true, ProcessPriorityClass priority = ProcessPriorityClass.Normal)
        {
            var process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = filePath;
            process.StartInfo.Arguments = arguments;

            if (asAdmin)
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
            }
            else if (waitForExit)
            {
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
            }

            process.Start();
            process.PriorityClass = priority;
            if (waitForExit)
                process.WaitForExit();
        }
    }
}
