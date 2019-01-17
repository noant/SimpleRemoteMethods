using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Protobuf wrapper for primitives
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ProtoContract]
    public struct DynamicTypeSurrogate<T>
    {
        [ProtoMember(1)]
        public T Value { get; set; }

        public DynamicTypeSurrogate(T val) => Value = val;
    }
}
