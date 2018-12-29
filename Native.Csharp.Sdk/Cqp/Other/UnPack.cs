using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Other
{
	/// <summary>
	/// 拆包类
	/// </summary>
	public class UnPack
	{
		#region --字段--
		private readonly byte[] _bytes = null;
		private int _index = 0;
		#endregion

		#region --属性--
		/// <summary>
		/// 获取当前解包数据的剩余长度
		/// </summary>
		public int OverLength
		{
			get
			{
				return this._bytes.Length - this._index + 1;
			}
		}
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 Native.Sdk.Cqp.Tool.Unpack 实例对象
		/// </summary>
		/// <param name="data">预处理的 byte[] </param>
		public UnPack(byte[] data)
		{
			this._bytes = data;
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 获取剩余所有数据
		/// </summary>
		/// <returns></returns>
		public byte[] GetAll()
		{
			return GetData(this._bytes.Length - this._index);
		}
		/// <summary>
		/// 获取指定长度 byte[] 
		/// </summary>
		/// <param name="len">欲获取数据的长度</param>
		/// <returns></returns>
		public byte[] GetBin(int len)
		{
			return this.GetData(len);
		}
		/// <summary>
		/// 获取一个 byte
		/// </summary>
		/// <returns></returns>
		public byte GetByte()
		{
			return this.GetData(1)[0];
		}
		/// <summary>
		/// 获取一个 Int16 数据
		/// </summary>
		/// <returns></returns>
		public short GetInt16()
		{
			return BitConverter.ToInt16(this.GetData(2, true), 0);
		}
		/// <summary>
		/// 获取一个 Int32 数据
		/// </summary>
		/// <returns></returns>
		public int GetInt32()
		{
			return BitConverter.ToInt32(this.GetData(4, true), 0);
		}
		/// <summary>
		/// 获取一个 Int64 数据
		/// </summary>
		/// <returns></returns>
		public long GetInt64()
		{
			return BitConverter.ToInt64(this.GetData(8, true), 0);
		}
		/// <summary>
		/// 获取一个 String 数据
		/// </summary>
		/// <param name="code">编码格式, 默认: Default</param>
		/// <returns></returns>
		public string GetString(Encoding code = null)
		{
			if (code == null)
			{
				code = Encoding.Default;
			}
			short len = this.GetInt16();
			return code.GetString(this.GetData(len));
		}
		/// <summary>
		/// 获取令牌
		/// </summary>
		/// <returns></returns>
		public byte[] GetToken()
		{
			short len = this.GetInt16();
			return GetBin(len);
		}
		#endregion

		#region --私有方法--
		/// <summary>
		/// 获取指定位数的 byte[], 并把游标向后移动指定长度
		/// </summary>
		/// <param name="len">长度</param>
		/// <param name="isReverse">是否反转, 默认: False</param>
		/// <returns></returns>
		private byte[] GetData(int len, bool isReverse = false)
		{
			byte[] temp = new byte[len];
			Buffer.BlockCopy(_bytes, _index, temp, 0, len);
			_index += len;
			return isReverse == true ? temp.Reverse().ToArray() : temp;
		}
		#endregion
	}
}
