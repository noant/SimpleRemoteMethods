using System;

namespace SimpleRemoteMethods.ServerSide
{
    public class Server<T>
    {
        public Server(T objectMethods)
        {
            Methods = objectMethods;
        }

        public T Methods { get; }

        public IAuthenticationValidator AuthenticationValidator = new AuthenticationValidatorStub();

        public ITokenDistributor TokenDistributor = new StandardTokenDistributor();

        public bool UseHttps { get; set; }
    }
}
