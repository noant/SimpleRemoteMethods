using System.Collections.Generic;
using System.Net;

namespace SimpleRemoteMethods.Bases
{
    public static class StatusCodesMatching
    {
        public static readonly Dictionary<HttpStatusCode, ErrorCode> HttpToSRM = new Dictionary<HttpStatusCode, ErrorCode>() {
            { HttpStatusCode.BadRequest, ErrorCode.UnknownData },
            { HttpStatusCode.NoContent, ErrorCode.DecryptionErrorCode },
        };

        public static readonly Dictionary<ErrorCode, HttpStatusCode> SRMToHttp = new Dictionary<ErrorCode, HttpStatusCode>() {
            { ErrorCode.UnknownData, HttpStatusCode.BadRequest },
            { ErrorCode.DecryptionErrorCode, HttpStatusCode.NoContent },
        };
    }
}