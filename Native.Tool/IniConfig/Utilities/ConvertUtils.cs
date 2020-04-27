using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Native.Tool.IniConfig.Utilities
{
	public static class ConvertUtils
	{
		/// <summary>
		/// 将字符串转换为 <see cref="TimeSpan"/>
		/// </summary>
		/// <param name="input">转换的字符串</param>
		/// <returns>转换成功返回 <see cref="TimeSpan"/></returns>
		internal static TimeSpan StringToTimeSpan (string input)
		{
			return TimeSpan.Parse (input, CultureInfo.InvariantCulture);
		}

		internal static long ToBigInteger (object objB)
		{
			throw new NotImplementedException ();
		}
	}
}
