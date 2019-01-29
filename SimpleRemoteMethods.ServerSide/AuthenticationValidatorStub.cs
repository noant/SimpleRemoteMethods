namespace SimpleRemoteMethods.ServerSide
{
    public class AuthenticationValidatorStub : IAuthenticationValidator
    {
        public bool Authenticate(string userName, string password)
        {
            return true;
        }
    }
}
