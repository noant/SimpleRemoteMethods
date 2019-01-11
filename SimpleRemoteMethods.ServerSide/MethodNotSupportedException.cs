using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    public class MethodNotSupportedException: Exception
    {
        public MethodNotSupportedException(string message):
            base(message)
        { }
    }
}
