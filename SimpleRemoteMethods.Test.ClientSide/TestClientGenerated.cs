using SimpleRemoteMethods.ClientSide;
using SimpleRemoteMethods.Test.Bases;
using System.Threading.Tasks;
using System;

namespace SimpleRemoteMethods.Test.ClientSide
{
    public class TestClientGenerated
    {
        public Client Client { get; }

        public TestClientGenerated(string host, ushort port, bool ssl, string secretKey, string login, string password, TimeSpan timeout = default(TimeSpan))
        {
            Client = new Client(host, port, ssl, secretKey, login, password, timeout);
        }

        public async Task TestMethod1()
        {
            await Client.CallMethod("TestMethod1");
        }

        public async Task TestMethod2(ITestParameter param, Int32 i, String g)
        {
            await Client.CallMethod("TestMethod2", param, i, g);
        }

        public async Task<ITestParameter> TestMethod3(String a, ITestParameter param)
        {
            return await Client.CallMethod<ITestParameter>("TestMethod3", a, param);
        }

        public async Task<Int32> TestMethod4(Int32 a)
        {
            return await Client.CallMethod<Int32>("TestMethod4", a);
        }

        public async Task<UInt16> TestMethod5(UInt16 a)
        {
            return await Client.CallMethod<UInt16>("TestMethod5", a);
        }

        public async Task<Object> TestMethod6(Object obj, ITestParameter param)
        {
            return await Client.CallMethod<Object>("TestMethod6", obj, param);
        }

        public async Task<Object> TestMethod6(ITestParameter param1, ITestParameter param2)
        {
            return await Client.CallMethod<Object>("TestMethod6", param1, param2);
        }

        public async Task<Object> TestMethod7(AbstractTestParameter2 param)
        {
            return await Client.CallMethod<Object>("TestMethod7", param);
        }

        public async Task<Object> TestMethod8(TestParameter<TestParameter> param)
        {
            return await Client.CallMethod<Object>("TestMethod8", param);
        }

        public async Task<TestParameter[]> TestMethod9(String s)
        {
            return await Client.CallMethodArray<TestParameter>("TestMethod9", s);
        }
    }
}
