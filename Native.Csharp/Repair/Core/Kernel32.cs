using Native.Csharp.Repair.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Csharp.Repair.Core
{
	/*
	 *	移植自: 00.00.dotnetRedirect 插件, 原作者: 成音S. 引用请带上此注释
	 *	论坛地址: https://cqp.cc/t/42920
	 */
	public static class Kernel32
	{
		[DllImport ("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs (UnmanagedType.Bool)]
		internal static extern bool AddDllDirectory (string lpPathName);

		[DllImport ("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs (UnmanagedType.Bool)]
		internal static extern bool SetDllDirectory (string lpPathName);

		[DllImport ("kernel32.dll")]
		internal static extern uint SetErrorMode (uint uMode);

		[DllImport ("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern IntPtr LoadLibrary (string dllToLoad);

		[DllImport ("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern IntPtr LoadLibraryEx (string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);
	}
}
