using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    internal static class Utils
    {
        internal static string GetErrorCodeDescription(ErrorCode code) =>
            $"Error code: {(byte)code} ({Enum.GetName(typeof(ErrorCode), code)})";
    }
}
