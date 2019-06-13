using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Native.Csharp.Tool.IniConfig.Linq
{
	/// <summary>
	/// 用于描述 Ini 配置项的类
	/// </summary>
	public class IniObject : Dictionary<string, IniSection>
	{
		#region --字段--
		private string _filePath = string.Empty;
		private Encoding _encoding = Encoding.Default;
		private static readonly Lazy<Regex[]> regices = new Lazy<Regex[]> (() => new Regex[]
		 {
			new Regex(@"^\[(.+)\]", RegexOptions.Compiled),						//匹配 节
			new Regex(@"^([^\r\n=]+)=((?:[^\r\n]+)?)",RegexOptions.Compiled)    //匹配 键值对
																				
			 //new Regex(@"^;(?:[\s\S]*)", RegexOptions.Compiled)				//匹配 注释
		 });
		#endregion

		#region --属性--
		/// <summary>
		/// 根据索引查找读取或设置与指定键关联的值
		/// </summary>
		/// <param name="index">键索引</param>
		/// <exception cref="ArgumentOutOfRangeException">index 小于 0 或大于或等于的中的元素数 source。</exception>
		/// <returns></returns>
		public IniSection this[int index]
		{
			get
			{
				return this[this.Keys.ElementAt (index)];
			}
			set
			{
				this[this.Keys.ElementAt (index)] = value;
			}
		}

		/// <summary>
		/// 根据指定的 "节" 名称读取或设置与指定键关联的值 (此索引器允许直接对不存在的键进行设置)
		/// </summary>
		/// <param name="name">要获取或设置的值的 "节" 名称</param>
		/// <returns></returns>
		public new IniSection this[string name]
		{
			get
			{
				return base[name];
			}
			set
			{
				if (this.ContainsKey (name))
				{
					base[name] = value;
				}
				else
				{
					this.Add (value);
				}
			}
		}

		/// <summary>
		/// 获取或设置用于读取或保存 Ini 配置项的 <see cref="System.Text.Encoding"/> 实例, 默认: ANSI
		/// </summary>
		public Encoding Encoding { get { return this._encoding; } set { this._encoding = value; } }

		/// <summary>
		/// 获取或设置用于保存 Ini 配置项的 <see cref="Uri"/> 实例
		/// </summary>
		public Uri Path { get; set; }

		/// <summary>
		/// 获取用于解析 Ini 配置项的 Regex 对象数组
		/// </summary>
		private static Regex[] Regices { get { return regices.Value; } }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="IniObject"/> 类的新实例, 该实例为空, 具有默认的初始容量并为键类型使用默认的相等比较器
		/// </summary>
		public IniObject ()
			: base ()
		{ }
		/// <summary>
		/// 初始化 <see cref="IniObject"/> 类的新实例, 该实例为空, 具有指定的初始容量并未键类型提供默认的相等比较器
		/// </summary>
		/// <param name="capacity"></param>
		public IniObject (int capacity)
			: base (capacity)
		{ }
		/// <summary>
		/// 初始化 <see cref="IniSection"/> 类的新实例, 该实例从包含指定的 <see cref="IDictionary{String, IniSection}"/> 赋值的元素并为键类型使用默认的相等比较器
		/// </summary>
		/// <param name="dictionary"><see cref="IDictionary{String, IniSection}"/>, 它的元素被复制到新 <see cref="IniObject"/> </param>
		public IniObject (IDictionary<string, IniSection> dictionary)
			: base (dictionary)
		{ }
		#endregion

		#region --公开方法--
		/// <summary>
		/// 将指定的 <see cref="IniSection"/> 添加到当前 <see cref="IniObject"/> 实例, 并以 <see cref="IniSection.Name"/> 作为键
		/// </summary>
		/// <param name="section">要添加到结尾的 <see cref="IniSection"/> 实例</param>
		public void Add (IniSection section)
		{
			base.Add (section.Name, section);
		}

		/// <summary>
		/// 创建一个数组, 从 <see cref="IniObject"/>
		/// </summary>
		/// <returns>返回当前实例内所有 <see cref="IniSection"/> 的集合</returns>
		public IniSection[] ToArray ()
		{
			return this.Values.ToArray ();
		}

		/// <summary>
		/// 将 Ini 配置项保存到指定的文件。如果存在指定文件，则此方法会覆盖它
		/// </summary>
		public void Save ()
		{
			if (this.Path == null)
			{
				throw new UriFormatException (string.Format ("Uri: {0}, 是无效的 Uri 对象", "IniObject.Path"));
			}
			this.Save (this.Path);
		}

		/// <summary>
		/// 将 Ini 配置项保存到指定的文件。 如果存在指定文件，则此方法会覆盖它。
		/// </summary>
		/// <param name="filePath">要将文档保存到其中的文件的位置。</param>
		public void Save (string filePath)
		{
			Save (new Uri (filePath));
		}

		/// <summary>
		/// 将 Ini 配置项保存到指定的文件。 如果存在指定文件，则此方法会覆盖它。
		/// </summary>
		/// <param name="fileUri">要将文档保存到其中的文件的位置。</param>
		public virtual void Save (Uri fileUri)
		{
			using (TextWriter textWriter = new StreamWriter (CheckinUri (fileUri), false, this.Encoding))
			{
				foreach (IniSection section in this.Values)
				{
					textWriter.WriteLine ("[{0}]", section.Name);
					foreach (KeyValuePair<string, IniValue> pair in section)
					{
						textWriter.WriteLine ("{0}={1}", pair.Key, pair.Value);
					}
					textWriter.WriteLine ();
				}
			}
		}

		/// <summary>
		/// 从文件以 ANSI 编码创建一个新的 IniObject 实例对象
		/// </summary>
		/// <param name="filePath">文件路径</param>
		/// <returns>转换成功返回 IniObject 实例对象</returns>
		public static IniObject Load (string filePath)
		{
			return Load (new Uri (filePath));
		}

		/// <summary>
		/// 从文件以 ANSI 编码创建一个新的 IniObject 实例对象
		/// </summary>
		/// <param name="fileUri">文件路径的 Uri 对象</param>
		/// <returns>转换成功返回 IniObject 实例对象</returns>
		public static IniObject Load (Uri fileUri)
		{
			return Load (fileUri, Encoding.Default);
		}

		/// <summary>
		/// 从文件以指定编码创建一个新的 IniObject 实例对象
		/// </summary>
		/// <param name="filePath">文件路径字符串</param>
		/// <param name="encoding">文件编码</param>
		/// <returns></returns>
		public static IniObject Load (string filePath, Encoding encoding)
		{
			return Load (new Uri (filePath), encoding);
		}

		/// <summary>
		/// 从文件以指定编码创建一个新的 IniObject 实例对象
		/// </summary>
		/// <param name="fileUri">文件路径的 Uri 对象</param>
		/// <param name="encoding">文件编码</param>
		/// <returns>转换成功返回 IniObject 实例对象</returns>
		public static IniObject Load (Uri fileUri, Encoding encoding)
		{
			string tempPath = CheckinUri (fileUri);

			//解释 Ini 文件
			using (TextReader textReader = new StreamReader (tempPath, encoding))
			{
				IniObject iObj = ParseIni (textReader);
				iObj.Path = fileUri;
				return iObj;
			}
		}

		/// <summary>
		/// 从字符串中创建一个新的 IniObject 实例对象
		/// </summary>
		/// <param name="iniStr">源字符串</param>
		/// <returns>转换成功返回 IniObject 实例对象</returns>
		public static IniObject Parse (string iniStr)
		{
			using (TextReader textReader = new StringReader (iniStr))
			{
				return ParseIni (textReader);
			}
		}
		#endregion

		#region --私有方法--
		/// <summary>
		/// 初始化文件路径
		/// </summary>
		/// <param name="fileUri"></param>
		/// <returns></returns>
		private static string CheckinUri (Uri fileUri)
		{
			if (!fileUri.IsAbsoluteUri)
			{
				string tempStr = string.Empty;
				if (fileUri.OriginalString.IndexOf ("\\", StringComparison.Ordinal) == 0)
				{
					tempStr = fileUri.OriginalString;
					tempStr = tempStr.Substring (1, tempStr.Length - 1);
				}
				else
				{
					tempStr = fileUri.OriginalString;
				}
				return System.IO.Path.Combine (AppDomain.CurrentDomain.BaseDirectory, tempStr);
			}
			else
			{
				try
				{
					if (fileUri.IsFile)
					{
						return fileUri.OriginalString;
					}
					else
					{
						throw new InvalidOperationException ("Uri 无效, 不允许为非文件 Uri");
					}
				}
				catch { throw; }
			}
		}

		/// <summary>
		/// 逐行解析 Ini 配置文件字符串
		/// </summary>
		/// <param name="textReader"></param>
		/// <returns></returns>
		private static IniObject ParseIni (TextReader textReader)
		{
			IniObject iniObj = new IniObject ();
			string key = string.Empty;
			while (textReader.Peek () != -1)
			{
				string line = textReader.ReadLine ();
				if (!string.IsNullOrEmpty (line))     //跳过空行
				{
					for (int i = 0; i < Regices.Length; i++)
					{
						Match match = Regices[i].Match (line);
						if (match.Success)
						{
							if (i == 0)
							{
								key = match.Groups[1].Value;
								iniObj.Add (new IniSection (key));
								break;
							}
							else if (i == 1)
							{
								iniObj[key].Add (match.Groups[1].Value.Trim (), match.Groups[2].Value);
							}
						}
					}
				}
			}
			return iniObj;
		}
		#endregion

		#region --重载方法--
		/// <summary>
		/// 将指定的键和值添加到 <see cref="IniObject"/> 的结尾处
		/// </summary>
		/// <param name="key">此变量无需使用</param>
		/// <param name="value"></param>
		public new void Add (string key, IniSection value)
		{
			base.Add (value.Name, value);
		}
		#endregion

		#region --重写方法--
		/// <summary>
		/// 将当前实例转换为等效的字符串
		/// </summary>
		/// <returns></returns>
		public override string ToString ()
		{
			StringBuilder iniString = new StringBuilder ();
			using (TextWriter textWriter = new StringWriter (iniString))
			{
				foreach (IniSection section in this.Values)
				{
					textWriter.WriteLine ("[{0}]", section.Name.Trim ());
					foreach (KeyValuePair<string, IniValue> pair in section)
					{
						textWriter.WriteLine ("{0}={1}", pair.Key.Trim (), pair.Value.Value.Trim ());
					}
					textWriter.WriteLine ();
				}
			}
			return iniString.ToString ();
		}
		#endregion
	}
}
