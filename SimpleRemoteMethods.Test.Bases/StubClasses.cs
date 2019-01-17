using ProtoBuf;
using SimpleRemoteMethods.Bases;
using System;
using System.Threading;

namespace SimpleRemoteMethods.Test.Bases
{
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
        [Remote]
        object TestMethod6(object obj, ITestParameter param);
        [Remote]
        object TestMethod6(ITestParameter param1, ITestParameter param2);
        [Remote]
        object TestMethod7(AbstractTestParameter2 param);
    }

    public interface ITestParameter
    {
        int Integer { get; set; }
    }

    [ProtoContract]
    public class TestParameter : ITestParameter
    {
        [ProtoMember(1)]
        public int Integer { get; set; } = 12;

        [ProtoMember(2)]
        public TestInner TestInner { get;set; }
    }

    [ProtoContract]
    public abstract class AbstractTestParameter2
    {
        public abstract string Tag { get; set; }
    }

    [ProtoContract]
    public class TestParameter2 : AbstractTestParameter2
    {
        public override string Tag { get; set; } = "TestValue";

        [ProtoMember(1)]
        public string TestProp { get; set; } = "TestValue2";
    }

    [ProtoContract]
    public class TestInner
    {
        [ProtoMember(1)]
        public string Test { get; set; }
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

        public object TestMethod6(object obj, ITestParameter param)
        {
            return obj;
        }

        public object TestMethod6(ITestParameter param1, ITestParameter param2)
        {
            return param1;
        }

        public object TestMethod7(AbstractTestParameter2 param)
        {
            if (param is TestParameter2 p)
                p.TestProp = DateTime.Now.ToShortDateString();
            return param;
        }
    }
}
