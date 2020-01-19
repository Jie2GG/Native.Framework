using Native.Csharp.Sdk.Cqp.Enum;
using Native.Csharp.Sdk.Cqp.Expand;
using Native.Csharp.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 表示 酷Q悬浮窗 的类
	/// </summary>
	public class CQFloatWindow : IToSendString
	{
		#region --属性--
		/// <summary>
		/// 获取或设置当前悬浮窗的值
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// 获取或设置当前悬浮窗使用的单位
		/// </summary>
		public string Unit { get; set; }

		/// <summary>
		/// 获取或设置当前悬浮窗的文本颜色
		/// </summary>
		public CQFloatWindowColors TextColor { get; set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="CQFloatWindow"/> 类的新实例
		/// </summary>
		public CQFloatWindow ()
			: this (string.Empty, string.Empty, CQFloatWindowColors.Black)
		{ }

		/// <summary>
		/// 初始化 <see cref="CQFloatWindow"/> 类的新实例
		/// </summary>
		/// <param name="value">用于展示的数据值</param>
		public CQFloatWindow (object value)
			: this (value, string.Empty, CQFloatWindowColors.Black)
		{ }

		/// <summary>
		/// 初始化 <see cref="CQFloatWindow"/> 类的新实例
		/// </summary>
		/// <param name="value">用于展示的数据值</param>
		/// <param name="unit">数据值的单位</param>
		public CQFloatWindow (object value, string unit)
			: this (value, unit, CQFloatWindowColors.Black)
		{ }

		/// <summary>
		/// 初始化 <see cref="CQFloatWindow"/> 类的新实例
		/// </summary>
		/// <param name="value">用于展示的数据值</param>
		/// <param name="unit">数据值的单位</param>
		/// <param name="textColor">文本颜色</param>
		public CQFloatWindow (object value, string unit, CQFloatWindowColors textColor)
		{
			if (value == null)
			{
				throw new ArgumentNullException ("value");
			}

			this.Value = value;
			this.Unit = unit;
			this.TextColor = textColor;
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 获取当前对象所描述的悬浮窗数据
		/// </summary>
		/// <returns>当前对象所描述的悬浮窗数据</returns>
		public string ToSendString ()
		{
			using (BinaryWriter writer = new BinaryWriter (new MemoryStream ()))
			{
				writer.Write_Ex (Convert.ToString (this.Value));
				writer.Write_Ex (this.Unit);
				writer.Write_Ex ((int)this.TextColor);
				return Convert.ToBase64String (writer.ToArray ());
			}
		}

		/// <summary>
		/// 确定指定的对象是否等于当前对象
		/// </summary>
		/// <param name="obj">要与当前对象进行比较的对象</param>
		/// <returns>如果指定的对象等于当前对象，则为 <code>true</code>，否则为 <code>false</code></returns>	
		public override bool Equals (object obj)
		{
			CQFloatWindow floatWindow = obj as CQFloatWindow;
			if (floatWindow != null)
			{
				return this.Value == floatWindow.Value && string.Equals (this.Unit, floatWindow.Unit) && this.TextColor == floatWindow.TextColor;
			}
			return base.Equals (obj);
		}

		/// <summary>
		/// 返回该字符串的哈希代码
		/// </summary>
		/// <returns> 32 位有符号整数哈希代码</returns>
		public override int GetHashCode ()
		{
			return base.GetHashCode () & this.Value.GetHashCode () & this.Unit.GetHashCode () & this.TextColor.GetHashCode ();
		}

		/// <summary>
		/// 返回当前对象的字符串
		/// </summary>
		/// <returns>当前对象的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("数据: {0}", this.Value != null ? this.Value : string.Empty));
			builder.AppendLine (string.Format ("单位: {0}", this.Unit != null ? this.Value : string.Empty));

			string temp = this.TextColor.GetDescription ();
			builder.AppendFormat ("颜色: {0}", temp != null ? this.Value : string.Empty);
			return builder.ToString ();
		}
		#endregion
	}
}
