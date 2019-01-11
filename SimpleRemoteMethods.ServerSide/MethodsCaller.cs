﻿using SimpleRemoteMethods.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    public class MethodsCaller<T>
    {
        private List<MemberInfoCacheItem> _cache = new List<MemberInfoCacheItem>();

        public MethodsCaller()
        {
            var allMethods = typeof(T).GetMethods();
            var remoteAttribute = typeof(RemoteAttribute);
            var remoteMethods = allMethods.Where(x => x.CustomAttributes.Any(x => x.AttributeType == remoteAttribute)).ToArray();
            if (remoteMethods.Any(x => x.GetParameters().Any(z => !z.IsIn)))
                throw new MethodNotSupportedException("Target methods cannot contains out or ref parameters");
        }

        public MethodCallResult Call(T methods, string name, object[] parameters, string returnTypeName)
        {
            var method = GetMethodInfo(methods, name, parameters.Select(x => x.GetType()).ToArray(), returnTypeName);
            if (method == null)
                return new MethodCallResult(null, null, true);
            try
            {
                var result = method.Invoke(methods, parameters);
                return new MethodCallResult(result, null, false);
            }
            catch(Exception e)
            {
                return new MethodCallResult(null, e, false);
            }
        }

        private MethodInfo GetMethodInfo(T methods, string name, Type[] parameters, string returnTypeName)
        {
            var item = _cache.FirstOrDefault(x => x.IsIt(name, parameters));
            if (item != null)
                return item.MemberInfo;

            var allMethods = typeof(T).GetMethods();
            var remoteAttribute = typeof(RemoteAttribute);
            var remoteMethods = allMethods.Where(x => x.CustomAttributes.Any(z => z.AttributeType == remoteAttribute)).ToArray();
            var targetMethod = remoteMethods.FirstOrDefault(x =>
                x.Name == name &&
                x.ReturnType.FullName == returnTypeName &&
                IsAllAssignable(parameters, x.GetParameters().Select(z => z.ParameterType).ToArray()));

            if (targetMethod != null)
                _cache.Add(new MemberInfoCacheItem(name, parameters, targetMethod));

            return targetMethod;
        }

        private bool IsAssignable(Type type1, Type type2)
        {
            if (type1 == type2)
                return true;
            return type1.IsAssignableFrom(type2);
        }

        private bool IsAllAssignable(Type[] types1, Type[] types2)
        {
            if (types1.Length != types2.Length)
                return false;

            for (int i = 0; i <= types1.Length; i++)
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