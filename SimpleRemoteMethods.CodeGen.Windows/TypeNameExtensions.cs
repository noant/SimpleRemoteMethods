using System;
using System.Collections.Generic;

namespace SimpleRemoteMethods.CodeGen.Windows
{
    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/4185521/c-sharp-get-generic-type-name
    /// </summary>
    public static class TypeNameExtensions
    {
        public static string GetFriendlyName(this Type type, List<Type> typeConstruction)
        {
            string friendlyName = type.Name;

            typeConstruction.Add(type);

            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetFriendlyName(typeParameters[i], typeConstruction);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }

            return friendlyName;
        }
    }
}
