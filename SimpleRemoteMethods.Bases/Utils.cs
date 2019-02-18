using System;
using System.Net;

namespace SimpleRemoteMethods.Bases
{
    internal static class Utils
    {
        internal static string GetErrorCodeDescription(ErrorCode code) =>
            $"Error code: {(byte)code} ({Enum.GetName(typeof(ErrorCode), code)})";

        internal static string GetInnerExceptionDetails(Exception e)
        {
            if (e == null)
            {
                return string.Empty;
            }

            if (e is WebException we)
            {
                return $"{e.Message} (StatusCode: {Enum.GetName(typeof(WebExceptionStatus), we.Status)})\r\n" +
                    GetInnerExceptionDetails(e.InnerException);
            }
            else
            {
                return e.Message + "\r\n" + GetInnerExceptionDetails(e.InnerException);
            }
        }
    }
}