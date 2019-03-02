using SimpleRemoteMethods.ServerSide;
using SimpleRemoteMethods.Test.Bases;
using SimpleRemoteMethods.Utils.Windows;
using System;
using System.Linq;
using System.Threading;

namespace SimpleRemoteMethods.Test.ServerSide
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TestServer();

            //TestServer_Stop();
            //TestServer_https();

            Console.ReadKey();
        }

        private static Server<ITestContracts> CreateServer(bool ssl = false)
        {
            var server = new Server<ITestContracts>(new TestContracts(), ssl, 8082, "0123456789123456");
            server.AuthenticationValidator = new AuthenticationValidatorTest();
            server.MaxConcurrentCalls = 5;
            server.TokenDistributor.TokenLifetime = TimeSpan.FromMinutes(1);
            server.LogRecord += (o, e) => Console.WriteLine(e.Exception?.ToString() ?? e.Message);

            server.MethodCall += Server_MethodCall;

            if (ssl)
            {
                var certHash = ServerHelper.GetInstalledCertificates().First().Hash;
                ServerHelper.PrepareHttpsServer(server, certHash, "testId");
            }
            else
            {
                ServerHelper.PrepareHttpServer(server, "testId");
            }

            return server;
        }

        private static void Server_MethodCall(object sender, RequestEventArgs e)
        {
            if (e.Request.Method == "TestMethod1" && e.UserName == "usr2")
            {
                e.ProhibitMethodExecution = true;
            }
        }

        private static void TestServer()
        {
            var server = CreateServer();
            server.StartAsync();
        }

        private static void TestServer_Stop()
        {
            var server = CreateServer();
            server.AfterServerStopped += (o, e) => Console.WriteLine("Server stopped...");
            server.StartAsync();
            Thread.Sleep(30000);
            server.Stop();
        }

        public static void TestServer_https()
        {
            var server = CreateServer(ssl: true);
            server.StartAsync();
        }

        private class AuthenticationValidatorTest : IAuthenticationValidator
        {
            public bool Authenticate(string userName, string password)
            {
                return (userName == "usr" || userName == "usr2") && password == "123123";
            }
        }
    }
}