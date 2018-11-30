using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Sdk.Cqp.Tool
{
	/// <summary>
	/// 时间操作类
	/// </summary>
	public static class Time
	{
		/// <summary>
		/// 将Unix时间戳转换为 System.DateTime 类型
		/// </summary>
		/// <param name="timeStamp">10 or 13 位时间戳</param>
		/// <returns></returns>
		public static DateTime FormatUnixTime(string timeStamp)
		{
			DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
			long lTime;
			if (timeStamp.Length.Equals(10))//判断是10位
			{
				lTime = long.Parse(timeStamp + "0000000");
			}
			else
			{
				lTime = long.Parse(timeStamp + "0000");//13位
			}
			TimeSpan toNow = new TimeSpan(lTime);
			DateTime daTime = dtStart.Add(toNow);
			return daTime;
		}
	}
}
