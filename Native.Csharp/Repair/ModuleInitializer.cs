using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Native.Csharp.Repair;
using Native.Csharp.Repair.Helper;

namespace Native.Csharp.Repair
{
	public static class ModuleInitializer
	{
		public static void Initialize()
		{
			// 注册程序集加载失败事件, 用于 Fody 库重定向的补充
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}

		/// <summary>
		/// 依赖库加载失败事件, 用于重定向到本项目下加载
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private static Assembly CurrentDomain_AssemblyResolve (object sender, ResolveEventArgs args)
		{
            if (args.Name.Split(',')[0].EndsWith(".resources"))
            {
                return null;
            }

            Assembly[] loadAssembly = AppDomain.CurrentDomain.GetAssemblies ();
			Assembly assembly = loadAssembly.Where (w => w.FullName.CompareTo (args.Name) == 0).LastOrDefault ();

			if (assembly != null)   // 不为 null 说明载入的库版本号相同, 则直接使用已载入的资源
			{
				return assembly;
			}

			assembly = args.RequestingAssembly != null ? args.RequestingAssembly : Assembly.GetExecutingAssembly ();

			if (string.IsNullOrEmpty (assembly.Location))
			{
				Uri uri = new Uri (assembly.CodeBase);
				if (uri.IsFile)
				{
					// 此实例为 酷Q data\tmp 目录下的组件
					if (File.Exists (uri.LocalPath))
					{
						assembly = Assembly.LoadFile (uri.LocalPath);
					}
				}
			}

			// 解析托管资源
			if (args.RequestingAssembly != null)
			{
				Assembly tmp = AssemblyHelper.AssemblyLoad (args.Name, assembly);
				if (tmp != null)
				{
					return tmp;
				}
			}

			// 解析非内嵌组件, 非托管, 可执行文件
			Uri uriOuter = new Uri (assembly.Location == null ? assembly.CodeBase : assembly.Location);
			if (!string.IsNullOrEmpty (uriOuter.LocalPath) && uriOuter.IsFile)
			{
				Queue<string> paths = new Queue<string> ();
				string path = Path.GetDirectoryName (uriOuter.LocalPath);
				if (Directory.Exists (path))
				{
					//存取本组件目录下的文件
					///coolq/data/tmp/cqpp/{guid}/...
					foreach (var f in Directory.GetFiles (path))
					{
						if (AssemblyHelper.IsDotNetAssembly (f))
						{
							paths.Enqueue (f);
						}
					}
				}
				//存取 酷Q /bin目录下的文件
				string bin = Path.Combine (Directory.GetCurrentDirectory (), "bin");
				if (Directory.Exists (bin))
				{
					foreach (var f in Directory.GetFiles (bin, "*.dll"))
					{
						if (AssemblyHelper.IsDotNetAssembly (f))
						{
							paths.Enqueue (f);
						}
					}
				}

				foreach (var file in paths)
				{
					if (File.Exists (file))
					{
						//托管组件
						try
						{
							AssemblyName assemblyName = AssemblyName.GetAssemblyName (file);
							Assembly tmp = Assembly.LoadFile (file);
							tmp = AssemblyHelper.AssemblyLoad (args.Name, tmp);
							if (tmp != null)
							{
								return tmp;
							}
						}
						catch { }
						//因为无法正确判定非托管组件，交回原请求者的重定向处理。
					}
				}
			}

			if (assembly.FullName == args.Name)
			{
				return assembly;
			}

			//若为内嵌文件(如:WPF .xaml, .resources)，尝试返回其请求者，自会进行解析。
			//若请求者为null，返回null交由下一AssemblyResolve处理，即最终交由原请求者的重定向处理。
			if (args.RequestingAssembly == Assembly.GetExecutingAssembly ())
			{
				return null;
			}
			return args.RequestingAssembly;
		}
	}
}
