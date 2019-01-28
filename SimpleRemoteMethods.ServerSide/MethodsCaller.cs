using ProtoBuf;
using SimpleRemoteMethods.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleRemoteMethods.ServerSide
{
    public class MethodsCaller<T>
    {
        private List<MemberInfoCacheItem> _cache = new List<MemberInfoCacheItem>();

        public MethodsCaller()
        {
            var allMethods = typeof(T).GetMethods();
            var remoteAttribute = typeof(RemoteAttribute);
            var remoteMethods = allMethods.Where(x => x.CustomAttributes.Any(z => z.AttributeType == remoteAttribute)).ToArray();

            if (remoteMethods.Any(x => x.IsGenericMethod))
                throw new MethodNotSupportedException("Target methods cannot be generic");

            if (remoteMethods.Any(x => x.GetParameters().Any(z => z.IsOut)))
                throw new MethodNotSupportedException("Target methods cannot contains out or ref parameters");
        }

        public MethodCallResult Call(T objectMethods, string name, object[] parameters, string returnTypeName)
        {
            parameters = parameters ?? new object[0];

            var methods = GetMethodInfo(objectMethods, name, parameters.Select(x => x?.GetType()).ToArray(), returnTypeName);

            if (methods.Length == 0)
                return new MethodCallResult(null, null, true, false);
            else if (methods.Length > 1)
                return new MethodCallResult(null, null, false, true);

            var method = methods.FirstOrDefault();

            try
            {
                var result = method.Invoke(objectMethods, parameters);
                if (method.ReturnType.GetCustomAttribute<ProtoContractAttribute>() == null &&
                    method.ReturnType.IsArray &&
                    method.ReturnType != typeof(string))
                    return new MethodCallResult(result as Array, null, false, false);
                return new MethodCallResult(result, null, false, false);
            }
            catch(Exception e)
            {
                return new MethodCallResult(null, e, false, false);
            }
        }

        private MethodInfo[] GetMethodInfo(T methods, string name, Type[] parameters, string returnTypeName)
        {
            var item = _cache.FirstOrDefault(x => x.IsIt(name, parameters));
            if (item != null)
                return new[] { item.MemberInfo };

            var allMethods = typeof(T).GetMethods();
            var remoteAttribute = typeof(RemoteAttribute);
            var remoteMethods = allMethods.Where(x => x.CustomAttributes.Any(z => z.AttributeType == remoteAttribute)).ToArray();
            var targetMethods = remoteMethods.Where(x =>
                x.Name == name &&
                x.ReturnType.FullName == returnTypeName &&
                IsAllAssignable(parameters, x.GetParameters().Select(z => z.ParameterType).ToArray()))
                .ToArray();
            
            if (targetMethods.Length == 1)
                _cache.Add(new MemberInfoCacheItem(name, parameters, targetMethods.FirstOrDefault()));

            return targetMethods;
        }

        private bool IsAssignable(Type type1, Type type2)
        {
            if (type1 == type2)
                return true;
            if (type1 == null && !type2.IsValueType)
                return true;
            return type2.IsAssignableFrom(type1);
        }

        private bool IsAllAssignable(Type[] types1, Type[] types2)
        {
            if (types1.Length != types2.Length)
                return false;

            for (int i = 0; i < types1.Length; i++)
                if (!IsAssignable(types1[i], types2[i]))
                    return false;

            return true;
        }

        private class MemberInfoCacheItem
        {
            public MemberInfoCacheItem(string methodName, Type[] parameters, MethodInfo methodInfo)
            {
                MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
                Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
                MemberInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            }

            public string MethodName { get; }
            public Type[] Parameters { get; }
            public MethodInfo MemberInfo { get; }

            public bool IsIt(string methodName, Type[] parameters)
            {
                return methodName == MethodName && Enumerable.SequenceEqual(parameters, Parameters);
            }
        }
    }
}
