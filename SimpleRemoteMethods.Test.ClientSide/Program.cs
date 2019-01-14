using SimpleRemoteMethods.ClientSide;
using SimpleRemoteMethods.Test.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.Test.ClientSide
{
    class Program
    {
        static void Main(string[] args)
        {
            //Thread.Sleep(10000);

            TestClient();

            Console.ReadKey();
        }

        private static async void TestClient()
        {
            var client = new Client("localhost", 8081, false, "1234123412341234", "usr", "");

            var param = new TestParameter();

            var task = client
                .CallMethod<ITestParameter>(nameof(ITestContracts.TestMethod3), "werfsdfasdfasdf", param);

            var res = await task;
        }
    }
}
