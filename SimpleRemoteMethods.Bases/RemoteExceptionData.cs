using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    public sealed class RemoteExceptionData
    {
        public RemoteExceptionData(string code, string message = "")
        {
            Code = code;
            Message = message;
        }

        public RemoteExceptionData() { }

        public string Message { get; set; }
        public string Code { get; set; }

        public const string DecryptionErrorCode = "0";
        public const string UnknownData = "1";
        public const string UserTokenExpired = "2";
        public const string LoginOrPasswordInvalid = "4";
    }
}
