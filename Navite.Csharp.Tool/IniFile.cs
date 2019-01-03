using Native.Csharp.Tool.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Native.Csharp.Tool
{
	/// <summary>
	/// Ini配置文件
	/// </summary>
	[Obsolete("请改用 IniConfig ")]
	public class IniFile
	{
		#region --字段--
		private readonly string _fileName;
		#endregion

		#region --属性--
		/// <summary>
		/// 获取当前Ini文件的绝对路径
		/// </summary>
		public string FileName
		{
			get { return _fileName; }
		}
		/// <summary>
		/// 获取文件是否存在
		/// </summary>
		public bool IsExists
		{
			get { return File.Exists(this.FileName); }
		}
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 Native.Csharp.Sdk.Cqp.Tool.IniFile 实例对象
		/// </summary>
		/// <param name="filePath">文件路径</param>
		public IniFile(string filePath)
		{
			this._fileName = filePath;
			if (!IsExists)
			{
				File.Create(this.FileName);
			}
		}
		#endregion

		#region --公开方法--

		#region --Write--
		/// <summary>
		/// 写入一个keyValuePair, 若 key 已存在, 则替换Value
		/// </summary>
		/// <param name="section">该键所在的节名称</param>
		/// <param name="key">该键的名称</param>
		/// <param name="Value">该键的值</param>
		public void Write(string section, string key, object Value)
		{
			Kernel32.WritePrivateProfileStringA(section, key, Value.ToString(), this.FileName);
		}
		#endregion

		#region --Read--
		/// <summary>
		/// 读取Ini文件指定节, 指定键中的值
		/// </summary>
		/// <param name="section">节</param>
		/// <param name="key">键</param>
		/// <param name="value">值, 当读取失败时, 返回该值</param>
		/// <returns></returns>
		public string Read(string section, string key, string value = null)
		{
			StringBuilder buffer = new StringBuilder(65535);
			Kernel32.GetPrivateProfileSectionA(section, buffer, buffer.Capacity, this.FileName);
			string str = buffer.ToString();
			if (string.IsNullOrEmpty(str))
			{
				return value;
			}
			return str;
		}
		/// <summary>
		/// 读取Ini文件指定节, 指定键中的值
		/// </summary>
		/// <param name="section">节</param>
		/// <param name="key">键</param>
		/// <param name="value">值, 当读取失败时, 返回该值</param>
		/// <returns></returns>
		public virtual int Read(string section, string key, int value = 0)
		{
			int result;
			try
			{
				result = int.Parse(this.Read(section, key, string.Empty));
			}
			catch
			{
				result = value;
			}
			return result;
		}
		/// <summary>
		/// 读取Ini文件指定节, 指定键中的值
		/// </summary>
		/// <param name="section">节</param>
		/// <param name="key">键</param>
		/// <param name="value">值, 当读取失败时, 返回该值</param>
		/// <returns></returns>
		public virtual long Read(string section, string key, long value = 0)
		{
			long result;
			try
			{
				result = long.Parse(this.Read(section, key, string.Empty));
			}
			catch
			{
				result = value;
			}
			return result;
		}
		/// <summary>
		/// 读取Ini文件指定节, 指定键中的值
		/// </summary>
		/// <param name="section">节</param>
		/// <param name="key">键</param>
		/// <param name="value">值, 当读取失败时, 返回该值</param>
		/// <returns></returns>
		public virtual byte Read(string section, string key, byte value = 0x00)
		{
			byte result;
			try
			{
				result = byte.Parse(this.Read(section, key, string.Empty));
			}
			catch
			{
				result = value;
			}
			return result;
		}
		/// <summary>
		/// 读取Ini文件指定节, 指定键中的值
		/// </summary>
		/// <param name="section">节</param>
		/// <param name="key">键</param>
		/// <param name="value">值, 当读取失败时, 返回该值</param>
		/// <returns></returns>
		public virtual float Read(string section, string key, float value = 0)
		{
			float result;
			try
			{
				result = float.Parse(this.Read(section, key, string.Empty));
			}
			catch
			{
				result = value;
			}
			return result;
		}
		/// <summary>
		/// 读取Ini文件指定节, 指定键中的值
		/// </summary>
		/// <param name="section">节</param>
		/// <param name="key">键</param>
		/// <param name="value">值, 当读取失败时, 返回该值</param>
		/// <returns></returns>
		public virtual double Read(string section, string key, double value = 0)
		{
			double result;
			try
			{
				result = double.Parse(this.Read(section, key, string.Empty));
			}
			catch
			{
				result = value;
			}
			return result;
		}
		/// <summary>
		/// 读取Ini文件指定节, 指定键中的值
		/// </summary>
		/// <param name="section">节</param>
		/// <param name="key">键</param>
		/// <param name="value">值, 当读取失败时, 返回该值</param>
		/// <returns></returns>
		public virtual bool Read(string section, string key, bool value = false)
		{
			bool result;
			try
			{
				result = bool.Parse(this.Read(section, key, string.Empty));
			}
			catch
			{
				result = value;
			}
			return result;
		}
		/// <summary>
		/// 读取Ini文件指定节, 指定键中的值
		/// </summary>
		/// <param name="section">节</param>
		/// <param name="key">键</param>
		/// <param name="value">值, 当读取失败时, 返回该值</param>
		/// <returns></returns>
		public virtual DateTime Read(string section, string key, DateTime value = new DateTime())
		{
			DateTime result;
			try
			{
				result = DateTime.Parse(this.Read(section, key, string.Empty));
			}
			catch
			{
				result = value;
			}
			return result;
		}
		/// <summary>
		/// 读取Ini文件指定节, 指定键中的值
		/// </summary>
		/// <param name="section">节</param>
		/// <param name="key">键</param>
		/// <param name="value">值, 当读取失败时, 返回该值</param>
		/// <returns></returns>
		public virtual TimeSpan Read(string section, string key, TimeSpan value = new TimeSpan())
		{
			TimeSpan result;
			try
			{
				result = TimeSpan.Parse(this.Read(section, key, string.Empty));
			}
			catch
			{
				result = value;
			}
			return result;
		}
		#endregion

		/// <summary>
		/// 读取Ini文件的所有节集合
		/// </summary>
		/// <returns></returns>
		public List<string> ReadSections()
		{
			byte[] buffer = new byte[65535];
			int rel = Kernel32.GetPrivateProfileSectionNamesA(buffer, buffer.GetUpperBound(0), this.FileName);
			int iCnt, iPos;
			List<string> arrayList = new List<string>();
			string tmp;
			if (rel > 0)
			{
				iCnt = 0; iPos = 0;
				for (iCnt = 0; iCnt < rel; iCnt++)
				{
					if (buffer[iCnt] == 0x00)
					{
						tmp = ASCIIEncoding.Default.GetString(buffer, iPos, iCnt).Trim();
						iPos = iCnt + 1;
						if (tmp != "")
							arrayList.Add(tmp);
					}
				}
			}
			return arrayList;
		}
		/// <summary>
		/// 判断指定的节是否存在
		/// </summary>
		/// <param name="section">节名称</param>
		///<returns></returns>
		public bool SectionExists(string section)
		{
			StringBuilder buffer = new StringBuilder(65535);
			Kernel32.GetPrivateProfileSectionA(section, buffer, buffer.Capacity, this.FileName);
			return buffer.ToString().Trim() == "";
		}
		/// <summary>
		/// 判断指定的节中指定的键是否存在
		/// </summary>
		/// <param name="section">节名称</param>
		/// <param name="key">键名称</param>
		/// <returns></returns>
		public bool ValueExits(string section, string key)
		{
			return Read(section, key, string.Empty).Trim() == "";
		}
		/// <summary>
		/// 删除指定的节中的指定键
		/// </summary>
		/// <param name="section">该键所在的节的名称</param>
		/// <param name="key">该键的名称</param>
		public void DeleteKey(string section, string key)
		{
			Write(section, key, null);
		}
		/// <summary>
		/// 删除指定的节的所有内容
		/// </summary>
		/// <param name="section">要删除的节的名字</param>
		public void DeleteSection(string section)
		{
			Kernel32.WritePrivateProfileSectionA(section, null, this.FileName);
		}
		/// <summary>
		/// 添加一个节
		/// </summary>
		/// <param name="section">要添加的节名称</param>
		public void AddSection(string section)
		{
			Kernel32.WritePrivateProfileSectionA(section, "", this.FileName);
		}
		/// <summary>
		/// 清空Ini配置文件
		/// </summary>
		public void Clear()
		{
			File.Delete(this.FileName);
			File.Create(this.FileName);
		}
		#endregion
	}
}
