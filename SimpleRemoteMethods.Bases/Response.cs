using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Response from server with custom data
    /// </summary>
    [ProtoContract]
    public class Response
    {
        /// <summary>
        /// Method called on server
        /// </summary>
        [ProtoMember(1)]
        public string Method { get; set; }

        /// <summary>
        /// Target data
        /// </summary>
        [ProtoIgnore]
        public object Result
        {
            get => ProtobufPrimitivesCreator.ExtractFromSurrogate(ResultSurrogate);
            set => ResultSurrogate = ProtobufPrimitivesCreator.CreateSurrogate(value);
        }

        /// <summary>
        /// Protobuf surrogate of Result property
        /// </summary>
        [ProtoMember(2, DynamicType = true)]
        public object ResultSurrogate { get; set; }

        /// <summary>
        /// Server time
        /// </summary>
        [ProtoMember(3)]
        public DateTime ServerTime { get; set; }
    }
}
