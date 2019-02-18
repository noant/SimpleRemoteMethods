using SimpleRemoteMethods.Bases;
using SimpleRemoteMethods.ClientSide;
using SimpleRemoteMethods.Test.Bases;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.Test.ClientSide
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.Sleep(2000);

            //TestClient_Forbidden();
            TestClient_MaxConcurrentCalls();
            //TestClient_ConnectionLease();
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
            //TestClient_Generic();
            //TestClient_Array();
            //TestClient_https();
            //TestClient_StrArray();
            //TestClient_StrArray2();
            //TestClient_LongData();

            Console.ReadKey();
        }

        private static TestClientGenerated CreateClient(string user = "usr", string pass = "123123", string secretCode = "1234123412341234", string host = "192.168.1.200")
        {
            var client = new TestClientGenerated(host, 8082, false, secretCode, user, pass, leaseTimeout: TimeSpan.FromMinutes(1));
            return client;
        }

        private async static void TestClient_RequestIdFabricationTest()
        {
            var client = CreateClient();

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
                var res = await HttpUtils.SendRequest(
                    new SafeHttpClient(client.Client.CallUri, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)),
                    request, "1234123412341234");
            }
            catch (RemoteException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async static void TestClient_RequestIdFabricationTest2()
        {
            var client = CreateClient();

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
                var res = await HttpUtils.SendRequest(
                    new SafeHttpClient(client.Client.CallUri, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)),
                    request, "1234123412341234");
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
                var res = await HttpUtils.SendRequest(
                    new SafeHttpClient(client.Client.CallUri, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)),
                    request, "1234123412341234");
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
                var client = CreateClient();
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
                var client = CreateClient(secretCode: "1234123412341235");
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
                var client = CreateClient(secretCode: "1234123412341235333"); // More than 16 symbols
                await client.TestMethod1();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async static void TestClient_BruteforceChecker()
        {
            var client = CreateClient(pass: "wrongpass");

            for (int i = 0; i <= 10; i++)
            {
                try
                {
                    await client.TestMethod1();
                }
                catch (RemoteException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private static void TestClient_MaxConcurrentCalls()
        {
            var param = new TestParameter();

            var client = CreateClient();

            var action = new Action(async () =>
            {
                for (int i = 0; i <= 80; i++)
                {
                    try
                    {
                        await client.TestMethod1();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });

            for (int j = 0; j <= 40; j++)
            {
                new Thread(() => action()).Start();
            }
        }

        private static void TestClient_ConnectionLease()
        {
            var client = CreateClient();
#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
            client.TestMethod1();
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
            Thread.Sleep(55 * 1000);
            for (int i = 0; i <= 80; i++)
            {
#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
                client.TestMethod1();
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
                Thread.Sleep(100);
            }
        }

        private async static void TestClient_SimpleMethod()
        {
            var client = CreateClient();
            var str = "asd";
            var res = await client.TestMethod3(str, new TestParameter());
            Console.WriteLine(res.Integer == str.Length ? "TestClient_SimpleMethod OK" : "TestClient_SimpleMethod Error!");
        }

        private async static void TestClient_ExceptionTransfer()
        {
            var client = CreateClient();

            await client.TestMethod2(new TestParameter(), 1, "132");

            try
            {
                await client.TestMethod2(null, 1, "123");
            }
            catch (RemoteException e) when (e.Code == ErrorCode.InternalServerError)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async static void TestClient_Ushort()
        {
            Console.WriteLine(await CreateClient().TestMethod5(5));
        }

        public async static void TestClient_ConflictDefinitions()
        {
            try
            {
                await CreateClient().TestMethod6(null, new TestParameter());
            }
            catch (RemoteException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async static void TestClient_AbstractClass()
        {
            var res = await CreateClient().TestMethod7(new TestParameter2());
            Console.WriteLine((res as TestParameter2).TestProp);
        }

        public async static void TestClient_Generic()
        {
            var res = await CreateClient().TestMethod8(new TestParameter<TestParameter>() { Obj = new TestParameter() { Integer = 25 } });
            Console.WriteLine((res as TestParameter).Integer);
        }

        public async static void TestClient_Forbidden()
        {
            try
            {
                await CreateClient(user: "usr2").TestMethod1();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async static void TestClient_Array()
        {
            try
            {
                var res = await CreateClient().TestMethod9("123");
                Console.WriteLine("Current datetime is " + DateTime.Now);

                foreach (var r in res)
                {
                    foreach (var s in r.Strs)
                    {
                        Console.WriteLine(s);
                    }
                }

                foreach (var r in res)
                {
                    Console.WriteLine(r.Dyn);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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

        public async static void TestClient_StrArray()
        {
            try
            {
                await CreateClient().TestMethod10(new[] { "asd", "asd2" });
                Console.WriteLine("Success!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async static void TestClient_StrArray2()
        {
            try
            {
                var strs = await CreateClient().TestMethod11(10);
                Console.WriteLine("Total strings received: " + strs.Length);
                foreach (var str in strs)
                {
                    Console.WriteLine(str);
                }

                Console.WriteLine("again with 0");
                strs = await CreateClient().TestMethod11(0);
                Console.WriteLine("Total strings received: " + strs.Length);
                foreach (var str in strs)
                {
                    Console.WriteLine(str);
                }

                Console.WriteLine("again with -1");
                strs = await CreateClient().TestMethod11(-1);
                Console.WriteLine("Total strings received: " + (strs?.Length.ToString() ?? "nothing"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async static void TestClient_LongData()
        {
            try
            {
                var result = await CreateClient().TestMethod12(9999999);
                Console.WriteLine("Result OK. Count: " + result.LongLength);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}