using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    public sealed class RemoteException: Exception
    {
        public static void Throw(string code, string message = "", Exception inner = null) => throw new RemoteException(new RemoteExceptionData(code, message), inner);
        public static void Throw(RemoteExceptionData data) => throw new RemoteException(data);

        public RemoteException(RemoteExceptionData data, Exception innerException = null):
            base(data.Message, innerException)
        {
            Data = data;
        }

        public new RemoteExceptionData Data { get; private set; }
    }
}
