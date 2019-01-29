using ProtoBuf;
using System;

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
            get => DynamicSurrogate.Extract(ResultSurrogate);
            set => ResultSurrogate = DynamicSurrogate.Create(value);
        }

        /// <summary>
        /// Protobuf surrogate of Result property
        /// </summary>
        [ProtoMember(2)]
        public DynamicSurrogate ResultSurrogate { get; set; }
        
        // There is problem while deserialize T[] arrays (where T is class with ProtoContractAttribute) packed in one object
        /// <summary>
        /// Target data if result type is array
        /// </summary>
        [ProtoIgnore]
        public object[] ResultArray
        {
            get => DynamicSurrogate.Extract(ResultSurrogateArray);
            set => ResultSurrogateArray = DynamicSurrogate.Create(value);
        }

        /// <summary>
        /// Protobuf surrogate of Result property
        /// </summary>
        [ProtoMember(3)]
        public DynamicSurrogate[] ResultSurrogateArray { get; set; }

        /// <summary>
        /// Server time
        /// </summary>
        [ProtoMember(4)]
        public DateTime ServerTime { get; set; }
    }
}
