using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    public class MethodCallResult
    {
        public MethodCallResult(object result, Exception callException, bool methodNotFound, bool moreThanOneMethodFound)
        {
            Result = result;
            CallException = callException;
            MethodNotFound = methodNotFound;
            MoreThanOneMethodFound = moreThanOneMethodFound;
        }

        public MethodCallResult(Array resultArray, Exception callException, bool methodNotFound, bool moreThanOneMethodFound)
        {
            ResultArray = resultArray;
            CallException = callException;
            MethodNotFound = methodNotFound;
            MoreThanOneMethodFound = moreThanOneMethodFound;
        }

        public object Result { get; }
        public Array ResultArray { get; }
        public Exception CallException { get; }
        public bool MethodNotFound { get; }
        public bool MoreThanOneMethodFound { get; }

        public bool IsEmpty => Result == null || ResultArray?.Length != 0;
    }
}
