using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Tool.IniConfig
{
	/// <summary>
	/// 表示 Ini 节点序列化的特性
	/// </summary>
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class IniSectionAttribute : Attribute
	{
		/// <summary>
		/// 节名称
		/// </summary>
		public string SectionName { get; set; }

		/// <summary>
		/// 初始化 <see cref="IniSectionAttribute"/> 类的新实例
		/// </summary>
		/// <param name="sectionName">用于表示节点的节名称</param>
		public IniSectionAttribute (string sectionName)
		{
			this.SectionName = sectionName;
		}
	}
}
