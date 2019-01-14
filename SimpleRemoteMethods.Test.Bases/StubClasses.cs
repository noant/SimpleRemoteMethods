using System;

namespace SimpleRemoteMethods.Test.Bases
{
    public interface ITestParameter
    {
        int Integer { get; set; }
    }

    public interface ITestContracts
    {
        void TestMethod1();
        void TestMethod2(ITestParameter param, int i, string g);
        ITestParameter TestMethod3(string a, ITestParameter param);
        int TestMethod4(int a);
    }

    public class TestParameter : ITestParameter
    {
        public int Integer { get; set; } = 12;
    }

    public class TestContracts : ITestContracts
    {
        public void TestMethod1()
        {
            Console.WriteLine("...TestMethod1");
        }

        public void TestMethod2(ITestParameter param, int i, string g)
        {
            Console.WriteLine("...TestMethod2");
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
    }
}
