using Native.Csharp.Sdk.Cqp.Enum;
using Native.Csharp.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Native.Csharp.Sdk.Cqp
{
	/// <summary>
	/// 消息解析
	/// </summary>
	public class CqMsg
	{
		#region --字段--
		private static Regex[] _regex = new Regex[]
		{
			new Regex(@"\[CQ:([A-Za-z]*)(?:(,[^\[\]]+))?\]", RegexOptions.Compiled),	// 匹配CQ码
			new Regex(@",([A-Za-z]+)=([^,\[\]]+)", RegexOptions.Compiled)				// 匹配键值对
		};
		#endregion

		#region --属性--
		/// <summary>
		/// 获取一个值, 该值是构造函数传入的原始值
		/// </summary>
		public string OriginalString { get; private set; }

		/// <summary>
		/// 获取本条消息中所有解析出的特殊内容
		/// </summary>
		public List<CqCode> Contents { get; private set; }
		#endregion

		#region --构造函数--
		private CqMsg (string message)
		{
			OriginalString = message;
			Contents = new List<CqCode> ();

			// 搜索消息中的 CQ码
			Parse ();
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 将所获取的消息内容序列化为 <see cref="CqMsg"/> 对象
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static CqMsg Parse (string message)
		{
			return new CqMsg (message);
		}
		#endregion

		#region --私有方法--
		private void Parse ()
		{
			MatchCollection matches = _regex[0].Matches (this.OriginalString);
			if (matches.Count > 0)
			{
				foreach (Match match in matches)
				{
					CqCode tempCode = new CqCode ();

					#region --初始化CqCode--
					tempCode.OriginalString = match.Groups[0].Value;
					tempCode.Index = match.Index;
					#endregion

					#region --解析CQ码类型--
					CqCodeType type = CqCodeType.Unknown;
					if (System.Enum.TryParse<CqCodeType> (match.Groups[1].Value, true, out type))
					{
						tempCode.Type = type;
					}
					#endregion

					#region --键值对解析--
					if (tempCode.Dictionary == null)
					{
						tempCode.Dictionary = new Dictionary<string, string> ();
					}
					MatchCollection kvResult = _regex[1].Matches (match.Groups[2].Value);
					foreach (Match kvMatch in kvResult)
					{
						tempCode.Dictionary.Add (kvMatch.Groups[1].Value, kvMatch.Groups[2].Value);
					}
					#endregion

					Contents.Add (tempCode);
				}
			}
		}
		#endregion
	}
}
