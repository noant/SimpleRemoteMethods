using SimpleRemoteMethods.ClientSide;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.Test.Bases
{
    /// <summary>
    /// NOT generated class
    /// </summary>
    public class ClientTest
    {
        public Client Client { get; set; }

        public async Task TestMethod1()
        {
            await Client.CallMethod(nameof(TestMethod1));
        }

        public async Task<ITestParameter> TestMethod3(string a, ITestParameter param)
        {
            return await Client.CallMethod<ITestParameter>(nameof(TestMethod3), new object[] { a, param });
        }

        public async Task TestMethod4(int a)
        {
            await Client.CallMethod(nameof(TestMethod4), new object[] { a });
        }

        public async Task TestMethod2(ITestParameter param, int i, string g)
        {
            await Client.CallMethod(nameof(TestMethod2), new object[] { param, i, g } );
        }

        public async Task<ushort> TestMethod5(ushort i)
        {
            return await Client.CallMethod<ushort>(nameof(TestMethod5), new object[] { i });
        }

        public async Task<object> TestMethod6(object obj, ITestParameter param)
        {
            return await Client.CallMethod<object>(nameof(TestMethod6), new object[] { obj, param });
        }

        public async Task<object> TestMethod7(AbstractTestParameter2 param)
        {
            return await Client.CallMethod<object>(nameof(TestMethod7), new object[] { param });
        }

        public async Task<object> TestMethod8(TestParameter<TestParameter> param)
        {
            return await Client.CallMethod<object>(nameof(TestMethod8), new object[] { param });
        }
    }
}
