using SimpleRemoteMethods.Bases;
using System;
using System.Threading;

namespace SimpleRemoteMethods.Test.Bases
{
    public interface ITestParameter
    {
        int Integer { get; set; }
    }

    public interface ITestContracts
    {
        [Remote]
        void TestMethod1();
        [Remote]
        void TestMethod2(ITestParameter param, int i, string g);
        [Remote]
        ITestParameter TestMethod3(string a, ITestParameter param);
        [Remote]
        int TestMethod4(int a);
        [Remote]
        ushort TestMethod5(ushort a);
    }

    public class TestParameter : ITestParameter
    {
        public int Integer { get; set; } = 12;
    }

    public class TestContracts : ITestContracts
    {
        public void TestMethod1()
        {
            Thread.Sleep(2000);
            Console.WriteLine("...TestMethod1");
        }

        public void TestMethod2(ITestParameter param, int i, string g)
        {
            Console.WriteLine("...TestMethod2");
            if (param == null)
                throw new NullReferenceException("Param cannot be null");
        }

        public ITestParameter TestMethod3(string a, ITestParameter param)
        {
            Console.WriteLine("...TestMethod3");
            param.Integer = a.Length;
            return param;
        }

        public int TestMethod4(int a)
        {
            Console.WriteLine("...TestMethod4");
            return a += 10;
        }

        public ushort TestMethod5(ushort a)
        {
            Console.WriteLine("...TestMethod5");
            return a += 10;
        }
    }
}
