using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Other
{
	/// <summary>
	/// 装包类
	/// </summary>
	public class Pack
	{
		#region --字段--
		private readonly List<byte> _bytes = null;
		#endregion

		#region --属性--
		/// <summary>
		/// 获取当前字节集长度
		/// </summary>
		public int Length
		{
			get { return this._bytes.Count; }
		}
		#endregion

		#region --构造函数--
		public Pack()
		{
			_bytes = new List<byte>();
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 取全部数据
		/// </summary>
		/// <returns></returns>
		public byte[] GetAll()
		{ return this._bytes.ToArray(); }
		/// <summary>
		/// 置byte[] 
		/// </summary>
		/// <param name="arr"></param>
		public void SetBin(byte[] arr)
		{
			this._bytes.AddRange(arr);
		}
		/// <summary>
		/// 置字节
		/// </summary>
		/// <param name="value"></param>
		public void SetByte(byte value)
		{
			this._bytes.Add(value);
		}
		/// <summary>
		/// 置类数据
		/// </summary>
		/// <param name="data"></param>
		public void SetData(byte[] data)
		{
			this._bytes.Clear();
			this._bytes.AddRange(data);

		}
		/// <summary>
		/// 置Int16值
		/// </summary>
		/// <param name="value"></param>
		public void SetInt16(short value)
		{
			this._bytes.AddRange(BitConverter.GetBytes(value));
		}
		/// <summary>
		/// 置Int32值
		/// </summary>
		/// <param name="value"></param>
		public void SetInt32(int value)
		{
			this._bytes.AddRange(BitConverter.GetBytes(value));
		}
		/// <summary>
		/// 置Int64值
		/// </summary>
		/// <param name="value"></param>
		public void SetInt64(long value)
		{
			this._bytes.AddRange(BitConverter.GetBytes(value));
		}
		/// <summary>
		/// 置字符串
		/// </summary>
		/// <param name="str">欲设置字符串</param>
		/// <param name="decode">解码格式</param>
		public void SetString(string str, Encoding decode = null)
		{
			if (decode == null)
			{
				decode = Encoding.Default;
			}
			this._bytes.AddRange(decode.GetBytes(str));
		}
		/// <summary>
		/// 置令牌字符串
		/// </summary>
		/// <param name="str"></param>
		/// <param name="decode"></param>
		public void SetLenString(string str, Encoding decode = null)
		{
			if (decode == null)
			{
				decode = Encoding.Default;
			}
			SetToken(decode.GetBytes(str));
		}
		/// <summary>
		/// 置令牌
		/// </summary>
		/// <param name="buf"></param>
		public void SetToken(byte[] buf)
		{
			SetInt16(Convert.ToInt16(buf.Length));
			SetBin(buf);
		}
		#endregion
	}
}
