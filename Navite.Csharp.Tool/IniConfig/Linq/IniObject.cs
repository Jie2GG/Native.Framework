using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Navite.Csharp.Tool.IniConfig.Linq
{
	/// <summary>
	/// 用于描述 Ini 配置项的类
	/// </summary>
	public class IniObject : List<IniSection>
	{
		#region --字段--
		private string _filePath = string.Empty;
		private static readonly Lazy<Regex[]> regices = new Lazy<Regex[]>(() => new Regex[]
		{
			new Regex(@"^\[(.+)\]", RegexOptions.Compiled),					//匹配 节
			new Regex(@"^(.+)=((?:[^\r\n]+)?)",RegexOptions.Compiled)		//匹配 键值对
			//new Regex(@"^;(?:[\s\S]*)", RegexOptions.Compiled)				//匹配 注释
		});
		#endregion

		#region --属性--
		/// <summary>
		/// 获取用于解析 Ini 配置项的 Regex 对象数组
		/// </summary>
		private static Regex[] Regices { get { return regices.Value; } }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 IniObject 类的新实例，该实例为空并且具有默认初始容量。
		/// </summary>
		public IniObject() : base()
		{ }
		/// <summary>
		/// 初始化 IniObject 类的新实例，该实例为空并且具有指定的初始容量。
		/// </summary>
		/// <param name="capacity">新配置项最初可以存储的元素数。</param>
		/// <exception cref="ArgumentOutOfRangeException">capacity 小于 0。</exception>
		public IniObject(int capacity) : base(capacity)
		{ }
		/// <summary>
		/// 初始化 IniObject 类的新实例，该实例包含从指定集合复制的元素并且具有足够的容量来容纳所复制的元素。
		/// </summary>
		/// <param name="collection">一个集合，其元素被复制到新列表中。</param>
		/// <exception cref="ArgumentNullException">collection 为 null。</exception>
		public IniObject(IEnumerable<IniSection> collection) : base(collection)
		{ }
		#endregion

		#region --公开方法--
		/// <summary>
		/// 将 Ini 配置项保存到指定的文件。 如果存在指定文件，则此方法会覆盖它。
		/// </summary>
		/// <param name="fileUri">要将文档保存到其中的文件的位置。</param>
		public void Save(string filePath)
		{
			Save(new Uri(filePath));
		}
		/// <summary>
		/// 将 Ini 配置项保存到指定的文件。 如果存在指定文件，则此方法会覆盖它。
		/// </summary>
		/// <param name="fileUri">要将文档保存到其中的文件的位置。</param>
		public virtual void Save(Uri fileUri)
		{
			using (TextWriter textWriter = new StreamWriter(CheckinUri(fileUri), false))
			{
				foreach (IniSection section in this)
				{
					textWriter.WriteLine("[{0}]", section.Name);
					foreach (KeyValuePair<string, IniValue> pair in section)
					{
						textWriter.WriteLine("{0}={1}", pair.Key, pair.Value);
					}
					textWriter.WriteLine();
				}
			}
		}

		/// <summary>
		/// 从文件创建一个新的 IniObject 实例对象
		/// </summary>
		/// <param name="filepPath">文件路径</param>
		/// <returns>转换成功返回 IniObject 实例对象</returns>
		public static IniObject Load(string filepPath)
		{
			return Load(new Uri(filepPath));
		}
		/// <summary>
		/// 从文件中创建一个新的 IniObject 实例对象
		/// </summary>
		/// <param name="fileUri">文件路径的 Uri 对象</param>
		/// <returns>转换成功返回 IniObject 实例对象</returns>
		public static IniObject Load(Uri fileUri)
		{
			string tempPath = CheckinUri(fileUri);

			//解释 Ini 文件语法
			using (TextReader textReader = File.OpenText(tempPath))
			{
				return ParseIni(textReader);
			}
		}

		/// <summary>
		/// 从字符串中创建一个新的 IniObject 实例对象
		/// </summary>
		/// <param name="iniStr">源字符串</param>
		/// <returns>转换成功返回 IniObject 实例对象</returns>
		public static IniObject Parse(string iniStr)
		{
			using (TextReader textReader = new StringReader(iniStr))
			{
				return ParseIni(textReader);
			}
		}
		#endregion

		#region --私有方法--
		/// <summary>
		/// 初始化文件路径
		/// </summary>
		/// <param name="fileUri"></param>
		/// <returns></returns>
		private static string CheckinUri(Uri fileUri)
		{
			if (!fileUri.IsAbsoluteUri)
			{
				string tempStr = string.Empty;
				if (fileUri.OriginalString.IndexOf("\\", StringComparison.Ordinal) == 0)
				{
					tempStr = fileUri.OriginalString;
					tempStr = tempStr.Substring(1, tempStr.Length - 1);
				}
				else
				{
					tempStr = fileUri.OriginalString;
				}
				return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempStr);
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
						throw new InvalidOperationException("Uri 无效, 不允许为非文件 Uri");
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
		private static IniObject ParseIni(TextReader textReader)
		{
			IniObject iniObj = new IniObject();
			while (textReader.Peek() != -1)
			{
				string line = textReader.ReadLine();
				if (!string.IsNullOrEmpty(line))     //跳过空行
				{
					for (int i = 0; i < Regices.Length; i++)
					{
						Match match = Regices[i].Match(line);
						if (match.Success)
						{
							if (i == 0)
							{
								iniObj.Add(new IniSection(match.Groups[1].Value));
								break;
							}
							else if (i == 1)
							{
								iniObj[iniObj.Count - 1].Add(match.Groups[1].Value.Trim(), match.Groups[2].Value);
							}
						}
					}
				}
			}
			return iniObj;
		}
		#endregion

		#region --重写方法--
		/// <summary>
		/// 将当前实例转换为等效的字符串
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder iniString = new StringBuilder();
			using (TextWriter textWriter = new StringWriter(iniString))
			{
				foreach (IniSection section in this)
				{
					textWriter.WriteLine("[{0}]", section.Name);
					foreach (KeyValuePair<string, IniValue> pair in section)
					{
						textWriter.WriteLine("{0}={1}", pair.Key, pair.Value);
					}
					textWriter.WriteLine();
				}
			}
			return iniString.ToString();
		}
		#endregion
	}
}
