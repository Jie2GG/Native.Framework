using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Native.Csharp.Repair;
using Native.Csharp.Repair.Core;
using Native.Csharp.Repair.Helper;

namespace Native.Csharp.Repair
{
    public static class ModuleInitializer
    {
        public static void Initialize ()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly ();
            Type typeLoader = executingAssembly.GetType ("Costura.AssemblyLoader");
            if (typeLoader == null)
            {
                return;
            }

            Dictionary<string, string> assemblyNames = ReflectionHelper.GetInstanceField<Dictionary<string, string>> (typeLoader, null, "assemblyNames");
            Dictionary<string, string> symbolNames = ReflectionHelper.GetInstanceField<Dictionary<string, string>> (typeLoader, null, "symbolNames");
            Uri uriOuter = new Uri (executingAssembly.Location == null ? executingAssembly.CodeBase : executingAssembly.Location);
            string path = Path.GetDirectoryName (uriOuter.LocalPath);
            string appPath = Path.Combine (path, executingAssembly.GetName ().Name);
            if (!Directory.Exists (path))
            {
                return;
            }
            Directory.CreateDirectory (appPath);

#pragma warning disable CS0618 // 类型或成员已过时
            AppDomain.CurrentDomain.AppendPrivatePath (appPath);
#pragma warning restore CS0618 // 类型或成员已过时

            Kernel32.AddDllDirectory (appPath);

            foreach (var assemblyName in assemblyNames)
            {
                byte[] rawAssembly;
                using (Stream stream = AssemblyHelper.LoadStream (assemblyName.Value, executingAssembly))
                {
                    if (stream != null)
                    {
                        rawAssembly = AssemblyHelper.ReadStream (stream);
                        File.WriteAllBytes (Path.Combine (appPath, assemblyName.Key + ".dll"), rawAssembly);
                    }
                }
            };

            foreach (var pdbName in symbolNames)
            {
                byte[] rawAssembly;
                using (Stream stream = AssemblyHelper.LoadStream (pdbName.Value, executingAssembly))
                {
                    if (stream != null)
                    {
                        rawAssembly = AssemblyHelper.ReadStream (stream);
                        File.WriteAllBytes (Path.Combine (appPath, pdbName.Key + ".pdb"), rawAssembly);
                    }
                }
            };
        }
    }
}