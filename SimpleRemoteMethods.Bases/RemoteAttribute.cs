using System;

namespace SimpleRemoteMethods.Bases
{
    /// <summary>
    /// Attribute for class methods that can be called from client
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RemoteAttribute: Attribute
    {

    }
}
