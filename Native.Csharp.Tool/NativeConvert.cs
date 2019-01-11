using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Csharp.Tool
{
	/// <summary>
	/// Native 用于数据转换的工具类
	/// </summary>
	public static class NativeConvert
	{
		/// <summary>
		/// 获取10或13位时间戳的 System.DateTime 表示形式
		/// </summary>
		/// <param name="timeStamp">10 or 13 位时间戳</param>
		/// <returns></returns>
		public static DateTime FotmatUnixTime (string timeStamp)
		{
			DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime (new DateTime (1970, 1, 1));
			long lTime;
			if (timeStamp.Length.Equals (10))//判断是10位
			{
				lTime = long.Parse (timeStamp + "0000000");
			}
			else
			{
				lTime = long.Parse (timeStamp + "0000");//13位
			}
			TimeSpan toNow = new TimeSpan (lTime);
			DateTime daTime = dtStart.Add (toNow);
			return daTime;
		}

		/// <summary>
		/// 获取字符串的 IntPtr 实例对象
		/// </summary>
		/// <param name="value">将转换的字符串</param>
		/// <param name="encoding">目标编码格式</param>
		/// <returns></returns>
		public static IntPtr ToStringPtr (string value, Encoding encoding = null)
		{
			if (encoding == null)
			{
				encoding = Encoding.Default;
			}
			byte[] buffer = encoding.GetBytes (value);
			GCHandle hobj = GCHandle.Alloc (buffer, GCHandleType.Pinned);
			return hobj.AddrOfPinnedObject ();
		}

		/// <summary>
		/// 获取指定 IntPtr 实例中的字符串
		/// </summary>
		/// <param name="strPtr">字符串的 IntPtr 对象</param>
		/// <param name="encoding">目标编码格式</param>
		/// <returns></returns>
		public static string ToPtrString (IntPtr strPtr, Encoding encoding = null)
		{
			if (encoding == null)
			{
				encoding = Encoding.Default;
			}
			string uniStr = Marshal.PtrToStringUni (strPtr);
			byte[] buffer = Encoding.Unicode.GetBytes (uniStr);
			Marshal.FreeHGlobal (strPtr);
			return encoding.GetString (buffer);
		}
	}
}
