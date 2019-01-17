using SimpleRemoteMethods.ServerSide;
using SimpleRemoteMethods.Test.Bases;
using SimpleRemoteMethods.Utils.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.Test.ServerSide
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestServer();
            //TestServer_Stop();

            TestServer_https();

            Console.ReadKey();
        }

        private static Server<ITestContracts> CreateServer()
        {
            var server = new Server<ITestContracts>(new TestContracts(), false, 8081, "1234123412341234");
            server.AuthenticationValidator = new AuthenticationValidatorTest();
            server.MaxConcurrentCalls = 5;
            server.TokenDistributor.TokenLifetime = TimeSpan.FromMinutes(1);
            server.LogRecord += (o, e) => Console.WriteLine(e.Exception?.ToString() ?? e.Message);
            return server;
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
            var server = new Server<ITestContracts>(new TestContracts(), true, 4041, "1234123412341234");
            server.AuthenticationValidator = new AuthenticationValidatorTest();
            server.MaxConcurrentCalls = 5;
            server.TokenDistributor.TokenLifetime = TimeSpan.FromMinutes(1);
            server.LogRecord += (o, e) => Console.WriteLine(e.Exception?.ToString() ?? e.Message);

            var certHash = ServerHelper.GetInstalledCertificates().First().Hash;

            ServerHelper.PrepareHttpsServer(server, certHash);
            server.StartAsync();
        }

        private class AuthenticationValidatorTest : IAuthenticationValidator
        {
            public bool Authenticate(string userName, string password)
            {
                return userName == "usr" && password == "123123";
            }
        }
    }
}
