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
            Thread.Sleep(2000);

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

        public class ClientTest
        {
            public Client Client { get; set; }
                        
            public void TestMethod1()
            {
                Client.CallMethod(nameof(TestMethod1));
            }

            public async Task<ITestParameter> TestMethod3(string a, ITestParameter param)
            {
                return await Client.CallMethod<ITestParameter>(nameof(TestMethod3), a, param);
            }

            public void TestMethod4(int a)
            {
                Client.CallMethod(nameof(TestMethod4), a);
            }

            public void TestMethod2(ITestParameter param, int i, string g)
            {
                Client.CallMethod(nameof(TestMethod2), param, i, g);
            }
        }
    }
}
