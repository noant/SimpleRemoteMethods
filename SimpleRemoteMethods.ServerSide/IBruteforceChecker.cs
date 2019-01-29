namespace SimpleRemoteMethods.ServerSide
{
    /// <summary>
    /// Prevent password bruteforce
    /// </summary>
    public interface IBruteforceChecker
    {
        /// <summary>
        /// Check last login activity and decides whether
        /// the user is trying to bruteforce a password
        /// </summary>
        /// <param name="loginString">Is client user name or ip</param>
        /// <returns></returns>
        bool CheckIsBruteforce(string loginString);

        /// <summary>
        /// Check the user or ip is in wait list
        /// </summary>
        /// <param name="loginString">Is client user name or ip</param>
        /// <returns></returns>
        bool IsWaitListContains(string loginString);
    }
}
