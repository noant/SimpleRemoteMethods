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


        // There is problem while deserialize T[] arrays (where T with ProtoContractAttribute) packed in one object
        /// <summary>
        /// Target data if result type is array
        /// </summary>
        [ProtoIgnore]
        public object[] ResultArray
        {
            get => ProtobufPrimitivesCreator.ExtractFromSurrogates(ResultSurrogateArray);
            set => ResultSurrogateArray = ProtobufPrimitivesCreator.CreateSurrogates(value);
        }

        /// <summary>
        /// Protobuf surrogate of Result property
        /// </summary>
        [ProtoMember(3, DynamicType = true)]
        public object[] ResultSurrogateArray { get; set; }

        /// <summary>
        /// Server time
        /// </summary>
        [ProtoMember(4)]
        public DateTime ServerTime { get; set; }
    }
}
