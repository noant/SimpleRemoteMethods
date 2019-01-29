using System;

namespace SimpleRemoteMethods.ServerSide
{
    public class MethodNotSupportedException: Exception
    {
        public MethodNotSupportedException(string message):
            base(message)
        { }
    }
}
