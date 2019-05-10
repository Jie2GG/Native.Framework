using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Native.Csharp.Repair.Core;
using Native.Csharp.Repair.Enum;

namespace Native.Csharp.Repair.Helper
{
	/*
	 *	移植自: 00.00.dotnetRedirect 插件, 原作者: 成音S. 引用请带上此注释
	 *	论坛地址: https://cqp.cc/t/42920
	 */
	public static class AssemblyHelper
	{
		#region --字段--
		private static List<string> preloaded = new List<string> ();
		#endregion

		#region --公开方法--
		public static Assembly ReadFromEmbeddedResources (Dictionary<string, string> assemblyNames, Dictionary<string, string> symbolNames, AssemblyName requestedAssemblyName, Assembly executingAssembly)
		{
			string text = requestedAssemblyName.Name.ToLowerInvariant ();
			if (requestedAssemblyName.CultureInfo != null && !string.IsNullOrEmpty (requestedAssemblyName.CultureInfo.Name))
			{
				text = string.Format ("{0}.{1}", requestedAssemblyName.CultureInfo.Name, text);
			}
			byte[] rawAssembly;
			using (Stream stream = LoadStream (assemblyNames, text, executingAssembly))
			{
				if (stream == null)
				{
					return null;
				}
				rawAssembly = ReadStream (stream);
			}
			using (Stream stream2 = LoadStream (symbolNames, text, executingAssembly))
			{
				if (stream2 != null)
				{
					byte[] rawSymbolStore = ReadStream (stream2);
					return Assembly.Load (rawAssembly, rawSymbolStore);
				}
			}
			return Assembly.Load (rawAssembly);
		}

		public static Assembly ReadFromEmbeddedResources (string name, Assembly executingAssembly)
		{
			string[] assemblys = executingAssembly.GetManifestResourceNames ();

			byte[] rawAssembly;
			foreach (var file in assemblys)
			{
				if (file.EndsWith (".resources"))
				{
					if (file == name.Split (',')[0])
					{
						return executingAssembly;
					}
				}
				if (file == name)
				{
					return executingAssembly;
				}
				if (file.EndsWith (".dll") || file.EndsWith (".dll.compressed"))
				{
					using (Stream stream = LoadStream (file, executingAssembly))
					{
						if (stream == null)
						{
							return null;
						}
						rawAssembly = ReadStream (stream);
					}
					Assembly tmp = Assembly.Load (rawAssembly);
					if (tmp.FullName == name)
					{
						return tmp;
					}
				}
			}
			return null;
		}

		public static Assembly CosturaAssemblyLoader (object sender, ResolveEventArgs args, Assembly executingAssembly)
		{
			Type typeLoader = executingAssembly.GetType ("Costura.AssemblyLoader");
			if (typeLoader != null)
			{
				if (Environment.OSVersion.Version.Major > 5 && Environment.OSVersion.Version.Minor > 2)
				{
					if (preloaded.Any () && !preloaded.Contains (executingAssembly.FullName))
					{
						preloaded.Add (executingAssembly.FullName);
						CosturaPreload (executingAssembly, typeLoader);
					}
				}
				//读取其打包后生成的组件目录并尝试载入。
				Dictionary<string, string> assemblyNames = ReflectionHelper.GetInstanceField<Dictionary<string, string>> (typeLoader, null, "assemblyNames");
				Dictionary<string, string> symbolNames = ReflectionHelper.GetInstanceField<Dictionary<string, string>> (typeLoader, null, "symbolNames");

				AssemblyName assemblyName = new AssemblyName (args.Name);
				Assembly embeddedAssembly = ReadFromEmbeddedResources (assemblyNames, symbolNames, assemblyName, executingAssembly);

				if (embeddedAssembly != null)
				{
					if (embeddedAssembly.FullName.CompareTo (args.Name) == 0)
					{
						return embeddedAssembly;
					}
				}

				//若非内嵌组件(即非托管或可执行文件)，尝试以原有重定向方法解析。
				embeddedAssembly = ReflectionHelper.InvokeMethod<Assembly> (typeLoader, null, "ResolveAssembly", new object[] { sender, args });
				if (embeddedAssembly != null)
				{
					if (embeddedAssembly.FullName.CompareTo (args.Name) == 0)
					{
						return embeddedAssembly;
					}
				}
			}
			return executingAssembly;
		}

		public static Assembly AssemblyLoad (string name, Assembly executingAssembly)
		{
			if (executingAssembly.FullName.CompareTo (name) == 0)
			{
				return executingAssembly;
			}

			//若组件为使用Native SDK，理应存在Fody.Costura 打包及重定向。
			//尝试解析其重定向类。
			executingAssembly = CosturaAssemblyLoader (null, new ResolveEventArgs (name), executingAssembly);
			if (executingAssembly.FullName.CompareTo (name) == 0)
			{
				return executingAssembly;
			}
			//若组件非Fody.Costura，尝试解析其内嵌组件。
			Assembly tmp = ReadFromEmbeddedResources (name, executingAssembly);
			if (tmp != null)
			{
				return tmp;
			}
			return null;
		}

		public static bool IsDotNetAssembly (string peFile)
		{
			uint peHeader;
			ushort dataDictionaryStart;
			uint[] dataDictionaryRVA = new uint[16];
			uint[] dataDictionarySize = new uint[16];

			Stream fs = new FileStream (peFile, FileMode.Open, FileAccess.Read);
			BinaryReader reader = new BinaryReader (fs);
			fs.Position = 0x3C;
			peHeader = reader.ReadUInt32 ();
			fs.Position = peHeader;
			dataDictionaryStart = Convert.ToUInt16 (Convert.ToUInt16 (fs.Position) + 0x60);
			fs.Position = dataDictionaryStart;
			try
			{
				for (int i = 0; i < 15; i++)
				{
					dataDictionaryRVA[i] = reader.ReadUInt32 ();
					dataDictionarySize[i] = reader.ReadUInt32 ();
				}
			}
			finally { fs.Position = 0; fs.Close (); }

			return dataDictionaryRVA[14] == 0;
		}

		public static void PreloadUnmanagedLibraries (Assembly executingAssembly, string hash, string tempBasePath, List<string> libs, Dictionary<string, string> checksums)
		{
			// since tempBasePath is per user, the mutex can be per user
			var mutexId = string.Format ("Costura{0}", hash);

			using (var mutex = new Mutex (false, mutexId))
			{
				var hasHandle = false;

				try
				{
					try
					{
						hasHandle = mutex.WaitOne (60000, false);
						if (hasHandle == false)
						{
							throw new TimeoutException ("等待独占访问的超时");
						}
					}
					catch (AbandonedMutexException)
					{
						hasHandle = true;
					}

					var bittyness = IntPtr.Size == 8 ? "64" : "32";
					CreateDirectory (Path.Combine (tempBasePath, bittyness));
					InternalPreloadUnmanagedLibraries (executingAssembly, tempBasePath, libs, checksums);
				}
				finally
				{
					if (hasHandle)
					{
						mutex.ReleaseMutex ();
					}
				}
			}
		}

		public static string CalculateChecksum (string filename)
		{
			using (var fs = new FileStream (filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
			{
				using (var bs = new BufferedStream (fs))
				{
					using (var sha1 = new SHA1CryptoServiceProvider ())
					{
						var hash = sha1.ComputeHash (bs);
						var formatted = new StringBuilder (2 * hash.Length);
						foreach (var b in hash)
						{
							formatted.AppendFormat ("{0:X2}", b);
						}
						return formatted.ToString ();
					}
				}
			}
		}

		public static void CopyTo (Stream source, Stream destination)
		{
			byte[] array = new byte[81920];
			int count;
			while ((count = source.Read (array, 0, array.Length)) != 0)
			{
				destination.Write (array, 0, count);
			}
		}

		public static byte[] ReadStream (Stream stream)
		{
			byte[] array = new byte[stream.Length];
			stream.Read (array, 0, array.Length);
			return array;
		}

		public static Stream LoadStream (string fullName, Assembly executingAssembly)
		{
			if (fullName.EndsWith (".compressed"))
			{
				using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream (fullName))
				{
					using (DeflateStream deflateStream = new DeflateStream (manifestResourceStream, CompressionMode.Decompress))
					{
						MemoryStream memoryStream = new MemoryStream ();
						CopyTo (deflateStream, memoryStream);
						memoryStream.Position = 0L;
						return memoryStream;
					}
				}
			}
			return executingAssembly.GetManifestResourceStream (fullName);
		}

		public static Stream LoadStream (Dictionary<string, string> resourceNames, string name, Assembly executingAssembly)
		{
			string fullName;
			if (resourceNames.TryGetValue (name, out fullName))
			{
				return LoadStream (fullName, executingAssembly);
			}
			return null;
		}
		#endregion

		#region --私有方法--
		private static void CosturaPreload (Assembly executingAssembly, Type typeLoader)
		{
			//读取其打包后生成的preload组件目录并尝试载入。
			List<string> preloadList = new List<string> ();
			List<string> preload32List = new List<string> ();
			List<string> preload64List = new List<string> ();   // 预兼容 64位 组件, 当酷Q支持 64时则生效
			Dictionary<string, string> checksums = new Dictionary<string, string> ();

			try
			{
				checksums = ReflectionHelper.GetInstanceField<Dictionary<string, string>> (typeLoader, null, "checksums");
				preload32List = ReflectionHelper.GetInstanceField<List<string>> (typeLoader, null, "preload32List");
				preload64List = ReflectionHelper.GetInstanceField<List<string>> (typeLoader, null, "preload64List");
				preloadList = ReflectionHelper.GetInstanceField<List<string>> (typeLoader, null, "preloadList");
			}
			catch { }
			if (checksums.Any ())
			{
				//var hash = $"{Process.GetCurrentProcess ().Id}_{executingAssembly.GetHashCode ()}";
				var hash = string.Format ("{0}_{1}", Process.GetCurrentProcess ().Id, executingAssembly.GetHashCode ());
				var prefixPath = Path.Combine (Path.GetTempPath (), "Costura");
				var tempBasePath = Path.Combine (prefixPath, hash);
				PreloadUnmanagedLibraries (executingAssembly, hash, tempBasePath, preloadList.Concat (preload32List).ToList (), checksums);
			}
		}

		private static void CreateDirectory (string tempBasePath)
		{
			if (!Directory.Exists (tempBasePath))
			{
				Directory.CreateDirectory (tempBasePath);
			}
		}

		private static string ResourceNameToPath (string lib)
		{
			var bittyness = IntPtr.Size == 8 ? "64" : "32";

			var name = lib;

			if (lib.StartsWith (string.Concat ("costura", bittyness, ".")))
			{
				name = Path.Combine (bittyness, lib.Substring (10));
			}
			else if (lib.StartsWith ("costura."))
			{
				name = lib.Substring (8);
			}

			if (name.EndsWith (".compressed"))
			{
				name = name.Substring (0, name.Length - 11);
			}

			return name;
		}

		private static void InternalPreloadUnmanagedLibraries (Assembly executingAssembly, string tempBasePath, IList<string> libs, Dictionary<string, string> checksums)
		{
			string name;

			foreach (var lib in libs)
			{
				name = ResourceNameToPath (lib);

				var assemblyTempFilePath = Path.Combine (tempBasePath, name);

				if (File.Exists (assemblyTempFilePath))
				{
					var checksum = CalculateChecksum (assemblyTempFilePath);
					if (checksum != checksums[lib])
					{
						File.Delete (assemblyTempFilePath);
					}
				}

				if (!File.Exists (assemblyTempFilePath))
				{
					using (var copyStream = LoadStream (lib, executingAssembly))
					{
						using (var assemblyTempFile = File.OpenWrite (assemblyTempFilePath))
						{
							CopyTo (copyStream, assemblyTempFile);
						}
					}
				}
			}

			//SetDllDirectory(tempBasePath);
			Kernel32.AddDllDirectory (tempBasePath);

			uint errorModes = 32771;
			var originalErrorMode = Kernel32.SetErrorMode (errorModes);

			foreach (var lib in libs)
			{
				name = ResourceNameToPath (lib);

				if (name.EndsWith (".dll"))
				{
					var assemblyTempFilePath = Path.Combine (tempBasePath, name);

					//LoadLibrary(assemblyTempFilePath);
					Kernel32.LoadLibraryEx (assemblyTempFilePath, IntPtr.Zero, LoadLibraryFlags.LOAD_LIBRARY_SEARCH_USER_DIRS);
				}
			}

			// restore to previous state
			Kernel32.SetErrorMode (originalErrorMode);
		}
		#endregion
	}
}
