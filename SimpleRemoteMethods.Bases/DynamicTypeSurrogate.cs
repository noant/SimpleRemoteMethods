using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    [ProtoContract]
    public struct DynamicTypeSurrogate<T>
    {
        [ProtoMember(1)]
        public T Value { get; set; }

        public DynamicTypeSurrogate(T val) => Value = val;
    }
}
