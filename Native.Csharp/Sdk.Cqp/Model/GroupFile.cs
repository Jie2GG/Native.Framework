using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 群文件
	/// </summary>
	public class GroupFile
	{
		/// <summary>
		/// 文件名
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// 文件Id
		/// </summary>
		public string Id { get; set; }
		/// <summary>
		/// Busid
		/// </summary>
		public int Busid { get; set; }
		/// <summary>
		/// 大小
		/// </summary>
		public long Size { get; set; }
	}
}
