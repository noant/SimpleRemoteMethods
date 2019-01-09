using System;

namespace SimpleRemoteMethods.ServerSide
{
    public class Server<T>
    {
        public Server(T objectMethods, bool useHttps = false, IAuthenticationValidator authentication, ITokenDistributor tokenDistributor)
        {
            Methods = objectMethods;
        }

        public T Methods { get; }

        public bool UseHttps { get; }
    }
}
