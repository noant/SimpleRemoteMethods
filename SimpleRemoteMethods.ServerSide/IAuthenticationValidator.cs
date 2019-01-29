namespace SimpleRemoteMethods.ServerSide
{
    /// <summary>
    /// User name/password authentitation
    /// </summary>
    public interface IAuthenticationValidator
    {
        /// <summary>
        /// Returns true, if application has a user and password
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool Authenticate(string userName, string password);
    }
}
