using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 表示酷Q悬浮窗回调事件参数的类
	/// </summary>
	public class CqStatusEventArgs : System.EventArgs
	{
		/// <summary>
		/// 悬浮窗ID
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// 悬浮窗名称
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// 悬浮窗标题
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// 更新间隔
		/// </summary>
		public int Period { get; private set; }

        /// <summary>
        /// 获取或设置当前悬浮窗显示的数据
        /// </summary>
        public string FloatWindowData { get; set; }

        /// <summary>
        /// 初始化 <see cref="CqStatusEventArgs"/> 类的一个新实例
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="name">名称</param>
        /// <param name="title">标题</param>
        /// <param name="period">更新时间</param>
        public CqStatusEventArgs (int id, string name, string title, int period)
		{
			this.Id = id;
			this.Name = name;
			this.Title = title;
			this.Period = period;
		}
	}
}
