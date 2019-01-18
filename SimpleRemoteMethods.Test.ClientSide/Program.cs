using SimpleRemoteMethods.Bases;
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

            //TestClient_MaxConcurrentCalls();
            //TestClient_BruteforceChecker();
            //TestClient_TokenExpiredTest();
            //TestClient_RequestIdFabricationTest();
            //TestClient_RequestIdFabricationTest2();
            //TestClient_WrongSecretKey();
            //TestClient_WrongSecretKey2();
            //TestClient_SimpleMethod();
            //TestClient_ExceptionTransfer();
            //TestClient_Ushort();
            //TestClient_ConflictDefinitions();
            //TestClient_AbstractClass();
            TestClient_Generic();
            //TestClient_https();

            Console.ReadKey();
        }

        private static ClientTest CreateClient(string pass, string secretCode = "1234123412341234")
        {
            var client = new Client("192.168.1.200", 8082, false, secretCode, "usr", pass);
            var testClient = new ClientTest() { Client = client };
            return testClient;
        }

        private async static void TestClient_RequestIdFabricationTest()
        {
            var client = CreateClient("123123");

            await client.TestMethod1(); // Auth crutch

            var requestId = Guid.NewGuid().ToString();

            try
            {
                var request = new Request();
                request.Method = nameof(ITestContracts.TestMethod1);
                request.RequestId = requestId;
                request.RequestIdRepeat = requestId + "FAKE";
                request.ReturnTypeName = typeof(void).FullName;
                request.UserToken = client.Client.CurrentUserToken;
                var res = await HttpUtils.SendRequest(new System.Net.Http.HttpClient(), client.Client.CallUri, request, "1234123412341234");
            }
            catch (RemoteException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async static void TestClient_RequestIdFabricationTest2()
        {
            var client = CreateClient("123123");

            await client.TestMethod1(); // Auth crutch

            var requestId = Guid.NewGuid().ToString();

            try
            {
                var request = new Request();
                request.Method = nameof(ITestContracts.TestMethod1);
                request.RequestId = requestId;
                request.RequestIdRepeat = requestId;
                request.ReturnTypeName = typeof(void).FullName;
                request.UserToken = client.Client.CurrentUserToken;
                var res = await HttpUtils.SendRequest(new System.Net.Http.HttpClient(), client.Client.CallUri, request, "1234123412341234");
            }
            catch (RemoteException e)
            {
                Console.WriteLine(e.ToString());
            }

            // Repeat with same request id

            try
            {
                var request = new Request();
                request.Method = nameof(ITestContracts.TestMethod1);
                request.RequestId = requestId;
                request.RequestIdRepeat = requestId;
                request.ReturnTypeName = typeof(void).FullName;
                request.UserToken = client.Client.CurrentUserToken;
                var res = await HttpUtils.SendRequest(new System.Net.Http.HttpClient(), client.Client.CallUri, request, "1234123412341234");
            }
            catch (RemoteException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async static void TestClient_TokenExpiredTest()
        {
            try
            {
                var client = CreateClient("123123");
                await client.TestMethod1();
                await Task.Delay(1000 * 61);
                await client.TestMethod1();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async static void TestClient_WrongSecretKey()
        {
            try
            {
                var client = CreateClient("123123", "1234123412341235");
                await client.TestMethod1();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async static void TestClient_WrongSecretKey2()
        {
            try
            {
                var client = CreateClient("123123", "1234123412341235333"); // More than 16 symbols
                await client.TestMethod1();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async static void TestClient_BruteforceChecker()
        {
            var client = CreateClient("wrongpass");

            for (int i = 0; i <= 10; i++)
                try
                {
                    await client.TestMethod1();
                }
                catch (RemoteException e)
                {
                    Console.WriteLine(e.ToString());
                }
        }

        private static void TestClient_MaxConcurrentCalls()
        {
            var client = CreateClient("123123");

            var param = new TestParameter();

            var action = new Action(async () => {
                for (int i = 0; i <= 40; i++)
                    await client.TestMethod1();
            });

            for (int j = 0; j <= 10; j++)
                new Thread(() => action()).Start();
        }

        private async static void TestClient_SimpleMethod()
        {
            var client = CreateClient("123123");
            var str = "asd";
            var res = await client.TestMethod3(str, new TestParameter());
            Console.WriteLine(res.Integer == str.Length ? "TestClient_SimpleMethod OK" : "TestClient_SimpleMethod Error!");
        }

        private async static void TestClient_ExceptionTransfer()
        {
            var client = CreateClient("123123");

            await client.TestMethod2(new TestParameter(), 1, "123");

            try
            {
                await client.TestMethod2(null, 1, "123");
            }
            catch (RemoteException e) when (e.Code == RemoteExceptionData.InternalServerError)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        public async static void TestClient_Ushort()
        {
            Console.WriteLine(await CreateClient("123123").TestMethod5(5));
        }

        public async static void TestClient_ConflictDefinitions()
        {
            try
            {
                await CreateClient("123123").TestMethod6(null, new TestParameter());
            }
            catch (RemoteException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async static void TestClient_AbstractClass()
        {
            var res = await CreateClient("123123").TestMethod7(new TestParameter2());
            Console.WriteLine((res as TestParameter2).TestProp);
        }

        public async static void TestClient_Generic()
        {
            var res = await CreateClient("123123").TestMethod8(new TestParameter<TestParameter>() { Obj = new TestParameter() { Integer = 25 } });
            Console.WriteLine((res as TestParameter).Integer);
        }

        public async static void TestClient_https()
        {
            try
            {
                var client = new Client("192.168.1.200", 4041, true, "1234123412341234", "usr", "123123");
                var testClient = new ClientTest() { Client = client };
                Console.WriteLine(await testClient.TestMethod5(5));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
