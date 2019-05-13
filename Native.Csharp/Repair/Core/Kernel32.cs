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
		// https://docs.microsoft.com/en-us/windows/desktop/api/libloaderapi/nf-libloaderapi-adddlldirectory
		[DllImport ("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs (UnmanagedType.Bool)]
		public static extern bool AddDllDirectory (string lpPathName);

		//https://docs.microsoft.com/en-us/windows/desktop/api/winbase/nf-winbase-setdlldirectorya
		[DllImport ("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs (UnmanagedType.Bool)]
		public static extern bool SetDllDirectory (string lpPathName);

		[DllImport ("kernel32.dll")]
		public static extern uint SetErrorMode (uint uMode);

		[DllImport ("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		static extern IntPtr LoadLibrary (string dllToLoad);

		[DllImport ("kernel32.dll", SetLastError = true)]
		public static extern IntPtr LoadLibraryEx (string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);
	}
}
