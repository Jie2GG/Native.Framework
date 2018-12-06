using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.App.Model
{
	public class AppInitializeEventArgs : EventArgs
	{
		/// <summary>
		/// 应用验证码
		/// </summary>
		public int AuthCode { get; set; }
	}
}
