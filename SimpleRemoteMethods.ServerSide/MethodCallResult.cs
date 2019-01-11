using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    public class MethodCallResult
    {
        public MethodCallResult(object result, Exception callException, bool methodNotFound)
        {
            Result = result;
            CallException = callException;
            MethodNotFound = methodNotFound;
        }

        public object Result { get; }
        public Exception CallException { get; }
        public bool MethodNotFound { get; }
    }
}
