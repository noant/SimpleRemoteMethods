using SimpleRemoteMethods.ServerSide;
using SimpleRemoteMethods.Test.Bases;
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
            TestServer();
            Console.ReadKey();
        }

        private static void TestServer()
        {
            var server = new Server<ITestContracts>(new TestContracts(), false, 8081, "1234123412341234");
            server.LogRecord += (o, e) => Console.WriteLine(e.Message ?? e.Exception?.Message ?? "???");
            server.StartAsync();
        }
    }
}
