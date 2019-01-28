using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Linq;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Protobuf wrapper for primitives
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ProtoContract]
    public class DynamicSurrogate<T>: DynamicSurrogate
    {
        [ProtoMember(1)]
        public T Value { get; set; }

        public DynamicSurrogate() { }

        public DynamicSurrogate(T val) => Value = val;
    }

    /// <summary>
    /// Protobuf wrapper for dynamic object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ProtoContract]
    public class DynamicObjectSurrogate : DynamicSurrogate
    {
        [ProtoMember(1, DynamicType = true, AsReference = true)]
        public object Value { get; set; }

        public DynamicObjectSurrogate() { }

        public DynamicObjectSurrogate(object val) => Value = val;
    }

    /// <summary>
    /// Wrapper for null
    /// </summary>
    [ProtoContract]
    public class NullSurrogate: DynamicSurrogate
    {
        // Empty
    }
    
    [ProtoContract]
    [ProtoInclude(101, typeof(DynamicSurrogate<byte>))]
    [ProtoInclude(102, typeof(DynamicSurrogate<sbyte>))]
    [ProtoInclude(103, typeof(DynamicSurrogate<short>))]
    [ProtoInclude(104, typeof(DynamicSurrogate<ushort>))]
    [ProtoInclude(105, typeof(DynamicSurrogate<int>))]
    [ProtoInclude(106, typeof(DynamicSurrogate<uint>))]
    [ProtoInclude(107, typeof(DynamicSurrogate<long>))]
    [ProtoInclude(108, typeof(DynamicSurrogate<ulong>))]
    [ProtoInclude(109, typeof(DynamicSurrogate<float>))]
    [ProtoInclude(110, typeof(DynamicSurrogate<double>))]
    [ProtoInclude(111, typeof(DynamicSurrogate<decimal>))]
    [ProtoInclude(112, typeof(DynamicSurrogate<bool>))]
    [ProtoInclude(113, typeof(DynamicSurrogate<char>))]
    [ProtoInclude(114, typeof(DynamicSurrogate<DateTime>))]
    [ProtoInclude(115, typeof(DynamicSurrogate<string>))] // Not primitive, but used there
    [ProtoInclude(116, typeof(DynamicObjectSurrogate))]
    [ProtoInclude(117, typeof(NullSurrogate))]
    public abstract class DynamicSurrogate
    {
        private static readonly NullSurrogate NullSurrogate = new NullSurrogate();

        private static readonly Type DynamicTypeSurrogate_GenericDefinition = typeof(DynamicSurrogate<>).GetGenericTypeDefinition();
        
        /// <summary>
        /// Create dynamic surrogate for object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DynamicSurrogate Create(object obj)
        {
            if (obj == null)
                return NullSurrogate;

            if (obj.GetType().IsClass && !(obj is string))
                return new DynamicObjectSurrogate(obj);

            DynamicSurrogate outParam = null;

            TypeSwitchPack<byte>(obj, ref outParam);
            TypeSwitchPack<sbyte>(obj, ref outParam);
            TypeSwitchPack<ushort>(obj, ref outParam);
            TypeSwitchPack<uint>(obj, ref outParam);
            TypeSwitchPack<ulong>(obj, ref outParam);
            TypeSwitchPack<byte>(obj, ref outParam);
            TypeSwitchPack<int>(obj, ref outParam);
            TypeSwitchPack<long>(obj, ref outParam);
            TypeSwitchPack<float>(obj, ref outParam);
            TypeSwitchPack<double>(obj, ref outParam);
            TypeSwitchPack<decimal>(obj, ref outParam);
            TypeSwitchPack<bool>(obj, ref outParam);
            TypeSwitchPack<char>(obj, ref outParam);
            TypeSwitchPack<DateTime>(obj, ref outParam);
            TypeSwitchPack<string>(obj, ref outParam);

            return outParam;
        }

        /// <summary>
        /// Create dynamic surrogates form objects
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static DynamicSurrogate[] Create(object[] objs) =>
            objs?.Select(x => Create(x)).ToArray();

        /// <summary>
        /// Extract object from surrogate
        /// </summary>
        /// <param name="surrogate"></param>
        /// <returns></returns>
        public static object Extract(DynamicSurrogate surrogate)
        {
            if (surrogate == null || surrogate is NullSurrogate)
                return null;

            if (surrogate is DynamicObjectSurrogate o)
                return o.Value;

            var surrogateType = surrogate.GetType();

            if (!surrogateType.IsGenericType)
                return surrogate;

            if (surrogateType.GetGenericTypeDefinition() != DynamicTypeSurrogate_GenericDefinition)
                return surrogate;

            return surrogateType
                .GetProperty(nameof(DynamicSurrogate<object>.Value))
                .GetValue(surrogate);
        }

        /// <summary>
        /// Extract objects from surrogates
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static object[] Extract(DynamicSurrogate[] objs) =>
            objs?.Select(x => Extract(x)).ToArray();

        private static void TypeSwitchPack<T>(object obj, ref DynamicSurrogate outParam)
        {
            if (outParam == null)
                if (obj is T)
                    outParam = new DynamicSurrogate<T>((T)obj);
        }

        private static void TypeSwitchExtract<T>(DynamicSurrogate objSurrogate, ref object outParam)
        {
            if (outParam == null)
                if (objSurrogate is DynamicSurrogate<T> surrogate)
                    outParam = surrogate.Value;
        }
    }
}
