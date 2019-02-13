using ProtoBuf;
using SimpleRemoteMethods.Bases;
using SimpleRemoteMethods.ServerSide;
using System;
using System.Collections.Generic;
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
        [Remote]
        object TestMethod8(TestParameter<TestParameter> param);
        [Remote]
        TestParameter[] TestMethod9(string s);
        [Remote]
        void TestMethod10(string[] s);
        [Remote]
        string[] TestMethod11(int cnt);
        [Remote]
        string[] TestMethod12(int cnt);
    }

    [ProtoContract]
    public interface ITestParameter
    {
        int Integer { get; set; }
    }

    [ProtoContract]
    public class TestParameter : ITestParameter
    {
        public TestParameter()
        {
            Dyn = new TestParameter2()
            {
                Tag = DateTime.Now.ToString()
            };
        }

        [ProtoMember(1)]
        public int Integer { get; set; } = 12;

        [ProtoMember(2)]
        public TestInner TestInner { get;set; }

        [ProtoMember(4, OverwriteList = true)]
        public string[] Strs { get; set; } = new[] { "a", "b", "c", DateTime.Now.ToString() };

        [ProtoIgnore]
        public object Dyn { get; set; }

        [ProtoMember(3)]
        public DynamicSurrogate DynProto
        {
            get => DynamicSurrogate.Create(Dyn);
            set => Dyn = DynamicSurrogate.Extract(value);
        }
    }

    [ProtoContract]
    public abstract class AbstractTestParameter2
    {
        public abstract string Tag { get; set; }
    }

    [ProtoContract]
    public class TestParameter<T>
    {
        [ProtoMember(1)]
        public T Obj { get; set; }
    }

    [ProtoContract]
    public class TestParameter2 : AbstractTestParameter2
    {
        [ProtoMember(1)]
        public override string Tag { get; set; } = "TestValue";

        [ProtoMember(2)]
        public string TestProp { get; set; } = "TestValue2";

        [ProtoIgnore]
        public object TestString { get; set; } = DateTime.Now.ToString() + " success!";

        [ProtoMember(3)]
        public DynamicSurrogate TestStringSurr
        {
            get => DynamicSurrogate.Create(TestString);
            set => TestString = DynamicSurrogate.Extract(value);
        }

        public override string ToString()
        {
            return Tag + " / " + TestString + "/" + base.ToString();
        }
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
            Console.WriteLine("Test getting request info from method:");
            Console.WriteLine("...current user: " + Server<ITestContracts>.CurrentRequestContext.UserName);
            Console.WriteLine("...current user ip: " + Server<ITestContracts>.CurrentRequestContext.ClientIp);
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

        public object TestMethod8(TestParameter<TestParameter> param)
        {
            return param.Obj;
        }

        public TestParameter[] TestMethod9(string s)
        {
            var list = new List<TestParameter>();
            list.Add(new TestParameter() { Integer = s.Length });
            list.Add(new TestParameter() { Integer = s.Length });
            Thread.Sleep(2000);
            return list.ToArray();
        }

        public void TestMethod10(string[] s)
        {
            foreach (var str in s)
                Console.WriteLine(str);
        }

        public string[] TestMethod11(int cnt)
        {
            if (cnt < 0)
                return null;
            var strs = new string[cnt];
            for (int i = 0; i < cnt; i++)
                strs[i] = DateTime.Now.Ticks.ToString();
            return strs;
        }

        public string[] TestMethod12(int cnt)
        {
            var list = new List<string>();
            for (int i = 0; i < cnt; i++)
                list.Add("test"+DateTime.Now.Ticks.ToString());
            return list.ToArray();
        }
    }
}
