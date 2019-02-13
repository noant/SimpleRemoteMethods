using System;
using System.IO;

namespace SimpleRemoteMethods.CodeGen.Windows
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("---");

            if (args == null || args.Length < 5)
                Console.WriteLine("Wrong input! Need to set [library path] [interface full name (with namespace) in target library] [generated class namespace] [generated class name] [export file path]");
            else
            {
                var path = Path.GetFullPath(args[4]);
                var libPath = Path.GetFullPath(args[0]);
                var interfaceName = args[1];
                var gNamespace = args[2];
                var gClassName = args[3];

                try
                {
                    Console.WriteLine("Code generation begin...");
                    var output = GenerateTool.GenerateClass(libPath, interfaceName, gNamespace, gClassName);
                    File.WriteAllText(path, output);
                    Console.WriteLine("Code generated successfully!");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while code generate...");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine();
                    Console.WriteLine("libPath " + libPath);
                    Console.WriteLine("interfaceName " + interfaceName);
                    Console.WriteLine("namespace " + gNamespace);
                    Console.WriteLine("className " + gClassName);
                    Console.WriteLine("exportTo " + path);
                }

                Console.WriteLine("---");
            }

            if (args.Length > 5 && args[5] == "d")
                Console.ReadKey();
        }
    }
}
