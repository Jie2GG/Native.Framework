using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Other
{
	/// <summary>
	/// 其它类扩展方法集
	/// </summary>
	public static class OtherExpand
	{
		#region --Kernel32--
		[DllImport ("kernel32.dll", EntryPoint = "lstrlenA", CharSet = CharSet.Ansi)]
		internal extern static int LstrlenA (IntPtr ptr);
        #endregion

        /// <summary>
        /// 获取 Unix 时间戳的 <see cref="DateTime"/> 表示形式
        /// </summary>
        /// <param name="unixTime">unix 时间戳</param>
        /// <returns></returns>
        public static DateTime ToDateTime (this int unixTime)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime (new DateTime (1970, 1, 1));
            TimeSpan toNow = new TimeSpan (long.Parse (string.Format ("{0}0000000", unixTime)));
            DateTime daTime = dtStart.Add (toNow);
            return daTime;
        }

        /// <summary>
        /// 转换字符串的 <see cref="IntPtr"/> 实例对象
        /// </summary>
        /// <param name="value">将转换的字符串</param>
        /// <param name="encoding">目标编码格式</param>
        /// <returns></returns>
        public static IntPtr ToIntPtr (this string source, Encoding encoding = null)
		{
			if (encoding == null)
			{
				encoding = Encoding.ASCII;
			}
			byte[] buffer = encoding.GetBytes (source);
			GCHandle hobj = GCHandle.Alloc (buffer, GCHandleType.Pinned);
			return hobj.AddrOfPinnedObject ();
		}

		/// <summary>
		/// 读取指针内所有的字节数组并编码为指定字符串
		/// </summary>
		/// <param name="strPtr">字符串的 <see cref="IntPtr"/> 对象</param>
		/// <param name="encoding">目标编码格式</param>
		/// <returns></returns>
		public static string ToString (this IntPtr strPtr, Encoding encoding = null)
		{
			if (encoding == null)
			{
				encoding = Encoding.Default;
			}

			int len = LstrlenA (strPtr);   //获取指针中数据的长度
			if (len == 0)
			{
				return string.Empty;
			}

			byte[] buffer = new byte[len];
			Marshal.Copy (strPtr, buffer, 0, len);
			return encoding.GetString (buffer);
		}
	}
}
