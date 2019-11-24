using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 提供用于描述酷Q菜单调用事件参数的类
	/// </summary>
	public class CQMenuCallEventArgs : CQMenuEventArgs
	{
		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="CQMenuCallEventArgs"/> 类的新实例
		/// </summary>
		/// <param name="name"></param>
		/// <param name="function"></param>
		public CQMenuCallEventArgs (string name, string function)
			: base (name, function)
		{
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>表示当前对象的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("名称: {0}", this.Name));
			builder.AppendLine (string.Format ("函数: {0}", this.Function));
			return builder.ToString ();
		}
		#endregion
	}
}
