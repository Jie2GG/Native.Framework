using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Tool
{
	public static class Converter
	{
		/// <summary>
		/// 获取10或13位时间戳的 System.DateTime 表示形式
		/// </summary>
		/// <param name="timeStamp">10 or 13 位时间戳</param>
		/// <returns></returns>
		public static DateTime FotmatUnixTime(string timeStamp)
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
