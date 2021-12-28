using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SimpleRemoteMethods.Utils.Windows
{
    public static class SecurityHelper
    {
        /// <summary>
        /// Bind certificate (installed in windows) to port
        /// </summary>
        /// <param name="certificateHash"></param>
        /// <param name="port"></param>
        public static void BindCertificateToPort(string certificateHash, ushort port, Action<string> resultLogging)
        {
            UnbindCertificatesFromPort(port, resultLogging);
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var cert = store
                .Certificates
                .Cast<X509Certificate2>()
                .FirstOrDefault(x => x.GetCertHashString().Equals(certificateHash));

            if (cert == null)
            {
                throw new Exception($"Cannot find certificate [{certificateHash}]");
            }

            var appid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;

            var command = $"http add sslcert ipport=0.0.0.0:{port} certhash={certificateHash} appid={{{appid}}}";
            ExecuteProcess(Path.Combine(Environment.SystemDirectory, "netsh.exe"), command, resultLogging: resultLogging);
        }

        /// <summary>
        /// Unbind certificate from port
        /// </summary>
        /// <param name="port"></param>
        public static void UnbindCertificatesFromPort(ushort port, Action<string> resultLogging)
        {
            var command = "http delete sslcert ipport=0.0.0.0:" + port;
            ExecuteProcess(Path.Combine(Environment.SystemDirectory, "netsh.exe"), command, resultLogging: resultLogging);
        }

        /// <summary>
        /// Reserve url in windows
        /// </summary>
        /// <param name="address"></param>
        public static void ReserveUrl(string address, Action<string> resultLogging)
        {
            RemoveAddressReservation(address, resultLogging: resultLogging);
            var normalizedAddress = address.TrimEnd('/') + "/";
            var commandString = $@"http add urlacl url=""{normalizedAddress}"" user=""{Environment.UserDomainName}\{Environment.UserName}""";

            ExecuteProcess(Path.Combine(Environment.SystemDirectory, "netsh.exe"), commandString, resultLogging: resultLogging);
        }

        /// <summary>
        /// Remove url reservation from windows
        /// </summary>
        /// <param name="address"></param>
        public static void RemoveAddressReservation(string address, Action<string> resultLogging)
        {
            var normalizedAddress = address.TrimEnd('/') + "/";
            var commandString = $@"http delete urlacl url=""{normalizedAddress}""";
            ExecuteProcess(Path.Combine(Environment.SystemDirectory, "netsh.exe"), commandString, resultLogging: resultLogging);
        }

        /// <summary>
        /// Add new certificate in windows
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string AddCertificateInWindows(string filename, string password, Action<string> resultLogging)
        {
            var certificate = new X509Certificate2(filename, password);
            var name = certificate.Subject.Replace("CN=", "");
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);

            var cert = new X509Certificate2(certificate);
            var existing = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
            if (existing?.Count > 0)
                store.RemoveRange(existing);
            store.Add(cert);
            store.Close();
            var hash = cert.GetCertHashString();
            resultLogging?.Invoke($"Certificate [{filename}][{hash}] added");
            return hash;
        }

        /// <summary>
        /// Create firewall rule for port and current executable
        /// </summary>
        /// <param name="port"></param>
        public static void AddFirewallRuleForPort(string ruleName, ushort port, Action<string> resultLogging)
        {
            var commandRemove = $"advfirewall firewall delete rule name=\"{ruleName}\"";
            var commandAdd =
                $"advfirewall firewall add rule name=\"{ruleName}\" action=allow " +
                $"dir=in enable=yes protocol=TCP localport={port} program=system";
            var netshpath = Path.Combine(Environment.SystemDirectory, "netsh.exe");
            ExecuteProcess(netshpath, commandRemove, resultLogging: resultLogging);
            ExecuteProcess(netshpath, commandAdd, resultLogging: resultLogging);
        }

        /// <summary>
        /// Remove firewall rule
        /// </summary>
        /// <param name="ruleName"></param>
        public static void RemoveFirewallRule(string ruleName, Action<string> resultLogging)
        {
            var commandRemove = $"advfirewall firewall delete rule name=\"{ruleName}\"";
            var netshpath = Path.Combine(Environment.SystemDirectory, "netsh.exe");
            ExecuteProcess(netshpath, commandRemove, resultLogging: resultLogging);
        }

        /// <summary>
        /// Execute process
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="arguments"></param>
        /// <param name="asAdmin"></param>
        /// <param name="waitForExit"></param>
        /// <param name="priority"></param>
        public static void ExecuteProcess(string filePath, string arguments, bool asAdmin = false, bool waitForExit = true, ProcessPriorityClass priority = ProcessPriorityClass.Normal, Action<string> resultLogging = null)
        {
            resultLogging?.Invoke($"Command [{filePath} {arguments}] start.");

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
                process.StartInfo.StandardErrorEncoding =
                    process.StartInfo.StandardOutputEncoding =
                    Encoding.GetEncoding(866);

                process.OutputDataReceived += (o, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        resultLogging?.Invoke($"Command [{filePath} {arguments}] output [{e.Data}].");
                    }
                };
            }

            process.Start();
            process.PriorityClass = priority;
            if (waitForExit)
            {
                process.BeginOutputReadLine();
                process.WaitForExit();
                resultLogging?.Invoke($"Command [{filePath} {arguments}] result code [{process.ExitCode}].");
            }
        }
    }
}