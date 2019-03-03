using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Csharp.Tool.Core
{
	internal static class Kernel32
	{
		#region --常量--
		private const string DllName = "kernel32.dll";
		#endregion

		[DllImport (DllName)]
		internal extern static int GetPrivateProfileIntA (string segName, string keyName, int iDefault, string fileName);

		[DllImport (DllName)]
		internal extern static int GetPrivateProfileStringA (string segName, string keyName, string sDefault, StringBuilder buffer, int nSize, string fileName);

		[DllImport (DllName)]
		internal extern static int GetPrivateProfileSectionA (string segName, StringBuilder buffer, int nSize, string fileName);

		[DllImport (DllName)]
		internal extern static int GetPrivateProfileSectionNamesA (byte[] buffer, int iLen, string fileName);

		[DllImport (DllName)]
		internal extern static int WritePrivateProfileSectionA (string segName, string sValue, string fileName);

		[DllImport (DllName)]
		internal extern static int WritePrivateProfileStringA (string segName, string keyName, string sValue, string fileName);

		[DllImport (DllName, EntryPoint = "lstrlenA", CharSet = CharSet.Ansi)]
		internal extern static int LstrlenA (IntPtr ptr);
	}
}
