using SimpleRemoteMethods.ClientSide;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.Test.Bases
{
    public class ClientTest
    {
        public Client Client { get; set; }

        public async Task TestMethod1()
        {
            await Client.CallMethod(nameof(TestMethod1));
        }

        public async Task<ITestParameter> TestMethod3(string a, ITestParameter param)
        {
            return await Client.CallMethod<ITestParameter>(nameof(TestMethod3), a, param);
        }

        public async Task TestMethod4(int a)
        {
            await Client.CallMethod(nameof(TestMethod4), a);
        }

        public async Task TestMethod2(ITestParameter param, int i, string g)
        {
            await Client.CallMethod(nameof(TestMethod2), param, i, g);
        }

        public async Task<ushort> TestMethod5(ushort i)
        {
            return await Client.CallMethod<ushort>(nameof(TestMethod5), i);
        }

        public async Task<object> TestMethod6(object obj, ITestParameter param)
        {
            return await Client.CallMethod<object>(nameof(TestMethod6), obj, param);
        }

        //public async Task<object> TestMethod6(ITestParameter param1, ITestParameter param2)
        //{
        //    return await Client.CallMethod<object>(nameof(TestMethod6), param1, param2);
        //}

        public async Task<object> TestMethod7(AbstractTestParameter2 param)
        {
            return await Client.CallMethod<object>(nameof(TestMethod7), param);
        }

        public async Task<object> TestMethod8(TestParameter<TestParameter> param)
        {
            return await Client.CallMethod<object>(nameof(TestMethod8), param);
        }
    }
}
