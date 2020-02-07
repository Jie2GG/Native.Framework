using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Tool.IniConfig
{
	/// <summary>
	/// 表示 Ini 键序列化的特性
	/// </summary>
	[AttributeUsage (AttributeTargets.Property)]
	public sealed class IniKeyAttribute : Attribute
	{
		/// <summary>
		/// 键名称
		/// </summary>
		public string KeyName { get; set; }

		/// <summary>
		/// 初始化 <see cref="IniKeyAttribute"/> 类的新实例
		/// </summary>
		/// <param name="keyName">用于表示键值对 "键" 的名称</param>
		public IniKeyAttribute (string keyName)
		{
			this.KeyName = keyName;
		}
	}
}
