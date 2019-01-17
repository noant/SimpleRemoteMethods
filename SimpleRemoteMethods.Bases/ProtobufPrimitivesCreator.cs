using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Create surrogates from primitives
    /// </summary>
    public static class ProtobufPrimitivesCreator
    {
        public static readonly NullSurrogate NullSurrogate = new NullSurrogate();

        public static object CreateSurrogate(object obj)
        {
            if (obj == null)
                return NullSurrogate;

            object outParam = null;

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

            return outParam ?? obj;
        }

        public static object[] CreateSurrogates(object[] objs) => 
            objs?.Select(x => CreateSurrogate(x)).ToArray();

        public static object ExtractFromSurrogate(object surrogate)
        {
            if (surrogate is NullSurrogate)
                return null;

            object outParam = null;

            TypeSwitchExtract<byte>(surrogate, ref outParam);
            TypeSwitchExtract<sbyte>(surrogate, ref outParam);
            TypeSwitchExtract<ushort>(surrogate, ref outParam);
            TypeSwitchExtract<uint>(surrogate, ref outParam);
            TypeSwitchExtract<ulong>(surrogate, ref outParam);
            TypeSwitchExtract<byte>(surrogate, ref outParam);
            TypeSwitchExtract<int>(surrogate, ref outParam);
            TypeSwitchExtract<long>(surrogate, ref outParam);
            TypeSwitchExtract<float>(surrogate, ref outParam);
            TypeSwitchExtract<double>(surrogate, ref outParam);
            TypeSwitchExtract<decimal>(surrogate, ref outParam);
            TypeSwitchExtract<bool>(surrogate, ref outParam);
            TypeSwitchExtract<char>(surrogate, ref outParam);

            return outParam ?? surrogate;
        }

        public static object[] ExtractFromSurrogates(object[] objs) =>
            objs?.Select(x => ExtractFromSurrogate(x)).ToArray();

        private static void TypeSwitchPack<T>(object obj, ref object outParam)
        {
            if (outParam == null)
                if (obj is T)
                    outParam = new DynamicTypeSurrogate<T>((T)obj);
        }

        private static void TypeSwitchExtract<T>(object objSurrogate, ref object outParam)
        {
            if (outParam == null)
                if (objSurrogate is DynamicTypeSurrogate<T> surrogate)
                    outParam = surrogate.Value;
        }
    }
}
