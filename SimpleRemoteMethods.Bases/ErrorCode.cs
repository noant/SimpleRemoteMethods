using ProtoBuf;

namespace SimpleRemoteMethods.Bases
{
    public enum ErrorCode: byte
    {
        DecryptionErrorCode = 0,
        UnknownData = 1,
        UserTokenExpired = 2,
        LoginOrPasswordInvalid = 4,
        RequestIdFabrication = 5,
        BruteforceSuspicion = 6,
        MethodNotFound = 7,
        MoreThanOneMethodFound = 8,
        InternalServerError = 9,
        ConnectionError = 10,
        TooMuchData = 11,
        AccessDenied = 12
    }
}
