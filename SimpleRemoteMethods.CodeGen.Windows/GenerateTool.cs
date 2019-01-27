using SimpleRemoteMethods.Bases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.CodeGen.Windows
{
    public static class GenerateTool
    {
        public static string GenerateClass(string path, string interfaceFullName, string generatedClassNamespace, string generatedClassName)
        {
            var sb = new StringBuilder();

            var codeLines = new List<string>();
            var usings = new List<string>();

            var assembly = Assembly.LoadFrom(path);
            var type = assembly.GetTypes().FirstOrDefault(x => x.IsInterface && x.Namespace + "." + x.Name == interfaceFullName);

            if (type == null)
                throw new Exception($"Type {interfaceFullName} not found");

            var methods = type.GetMethods().Where(x => x.GetCustomAttributes().Any(z => z is RemoteAttribute)).ToArray();

            if (methods.Length == 0)
                throw new Exception("Methods with [Remote] attribute not found");

            usings.Add("using System;");
            usings.Add("using System.Threading.Tasks;");
            usings.Add("using SimpleRemoteMethods.ClientSide;");
            codeLines.Add(string.Empty);
            codeLines.Add("namespace " + generatedClassNamespace);
            codeLines.Add("{");
            codeLines.Add("    public class " + generatedClassName);
            codeLines.Add("    {");
            codeLines.Add("        public Client Client { get; }");
            codeLines.Add(string.Empty);
            codeLines.Add($"        public {generatedClassName}(string host, ushort port, bool ssl, string secretKey, string login, string password, TimeSpan timeout = default(TimeSpan))");
            codeLines.Add("        {");
            codeLines.Add("            Client = new Client(host, port, ssl, secretKey, login, password, timeout);");
            codeLines.Add("        }");

            foreach (var method in methods)
            {
                var returnType = method.ReturnType;
                var typeConstruction = new List<Type>();
                var returnTypeName = returnType.GetFriendlyName(typeConstruction);
                AppendUsings(typeConstruction, usings, generatedClassNamespace);
                codeLines.Add(string.Empty);
                var attrs = "        public async Task";
                if (returnType != typeof(void))
                    attrs += "<" + returnTypeName + ">";

                var parameters = method.GetParameters().OrderBy(x => x.Position).ToArray();
                var methodName = method.Name;

                if (parameters.Length == 0)
                    methodName += "()";
                else
                {
                    var paramsStr = "";
                    foreach (var param in parameters)
                    {
                        if (param.IsOut)
                            throw new Exception("Method parameter cannot contains [out] or [ref] attributes");

                        var paramName = param.Name;
                        var paramType = param.ParameterType;
                        var paramTypeConstruction = new List<Type>();
                        var paramTypeName = paramType.GetFriendlyName(paramTypeConstruction);

                        AppendUsings(paramTypeConstruction, usings, generatedClassNamespace);

                        paramsStr += ", " + paramTypeName + " " + paramName;
                    }
                    methodName += "(" + paramsStr.Substring(2) + ")";
                }

                codeLines.Add(attrs + " " + methodName);
                codeLines.Add("        {");

                var paramsUsage = "";

                string call = "";
                if (returnType == typeof(void))
                    call = "await Client.CallMethod";
                else if (returnType.IsArray)
                {
                    var innerType = returnType.GetElementType();
                    AppendUsings(new List<Type> { innerType }, usings, generatedClassNamespace);
                    call = $"return await Client.CallMethodArray<{innerType.Name}>";
                }
                else
                {
                    call = $"return await Client.CallMethod<{returnTypeName}>";
                }

                if (parameters.Length != 0)
                {
                    paramsUsage = parameters.Select(x => x.Name).Aggregate((x1, x2) => x1 + ", " + x2);
                    call += "(\"" + method.Name + "\", " + paramsUsage + ");";
                }
                else
                    call += "(\"" + method.Name + "\");";

                codeLines.Add("            "+call);
                codeLines.Add("        }");
            }

            codeLines.Add("    }");
            codeLines.Add("}");

            foreach (var @using in usings.OrderBy(x => x))
            {
                Console.WriteLine(@using);
                sb.AppendLine(@using);
            }

            foreach(var codeLine in codeLines)
            {
                Console.WriteLine(codeLine);
                sb.AppendLine(codeLine);
            }

            return sb.ToString();
        }

        private static void AppendUsings(List<Type> types, List<string> allUsings, string mainNamespace)
        {
            foreach (var type in types)
            {
                var @using = "using " + type.Namespace + ";";
                if (!allUsings.Contains(@using) && type.Namespace != mainNamespace)
                    allUsings.Add(@using);
            }
        }
    }
}
