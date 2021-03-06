﻿using ProtoBuf;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Request to server to get cusom data
    /// </summary>
    [ProtoContract]
    public class Request
    {
        /// <summary>
        /// Unique requst id
        /// </summary>
        [ProtoMember(1)]
        public string RequestId { get; set; }

        /// <summary>
        /// Method called on server
        /// </summary>
        [ProtoMember(2)]
        public string Method { get; set; }

        /// <summary>
        /// Method input parameters
        /// </summary>
        [ProtoIgnore]
        public object[] Parameters { get; set; }

        /// <summary>
        /// Surrogates for ProtoBuf
        /// </summary>
        [ProtoMember(3)]
        public DynamicSurrogate[] ParametersSurrogates
        {
            get => DynamicSurrogate.Create(Parameters);
            set => Parameters = DynamicSurrogate.Extract(value);
        }

        /// <summary>
        /// Name of object class that method returns
        /// </summary>
        [ProtoMember(4)]
        public string ReturnTypeName { get; set; }

        /// <summary>
        /// Authentication user token
        /// </summary>
        [ProtoMember(5)]
        public string UserToken { get; set; }

        /// Intruder can change request id even if it encrypted by changing encrypted bytes to random
        /// in place where RequestId parameter setted; repeat of request id and checking it on server side
        /// reduces to nothing intruder attemts to change request id. Not actual for CBC cipher mode.
        /// <summary>
        /// Unique requst id repeat
        /// </summary>
        [ProtoMember(6)]
        public string RequestIdRepeat { get; set; }
    }
}
