using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace Native.Csharp.Tool.Http
{
	/**
	 *	Http访问的操作类来自 Flexlive.CQP.Framework 框架. 若有侵权请联系我删除重写
	 */
	/// <summary>
	/// Http访问的操作类
	/// </summary>
	public static class HttpHelper
	{
		/// <summary>
		/// 向HTTP服务器发送Get请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="referer">参照页</param>
		/// <param name="userAgent">User-Agent HTTP标头</param>
		/// <param name="accept">Accept HTTP标头</param>
		/// <param name="timeout">超时时间</param>
		/// <param name="header">HTTP 标头</param>
		/// <param name="cookies">Cookies</param>
		/// <param name="encoding">文本编码</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Get(string url, string referer, string userAgent, string accept, int timeout, WebHeaderCollection header, CookieCollection cookies, Encoding encoding, DecompressionMethods decompression = DecompressionMethods.None)
		{
			string result = string.Empty;
			try
			{
				HttpWebRequest httpWebRequest = null;
				if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
				{
					ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
					httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
					httpWebRequest.ProtocolVersion = HttpVersion.Version10;
				}
				else
				{
					httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
				}
				httpWebRequest.UserAgent = userAgent;
				httpWebRequest.Referer = referer;
				httpWebRequest.Method = "GET";
				httpWebRequest.Timeout = timeout;
				httpWebRequest.Accept = accept;
				if (cookies != null)
				{
					httpWebRequest.CookieContainer = new CookieContainer();
					httpWebRequest.CookieContainer.Add(cookies);
				}
				if (header != null)
				{
					httpWebRequest.Headers = header;
				}
				httpWebRequest.AutomaticDecompression = decompression;
				HttpWebResponse webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				result = GetResponseString(webResponse, encoding);
				return result;
			}
			catch
			{
				return result;
			}
		}

		/// <summary>
		/// 向HTTP服务器发送Get请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="referer">参照页</param>
		/// <param name="header">HTTP 标头</param>
		/// <param name="cookies">Cookies</param>
		/// <param name="encoding">文本编码</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Get(string url, string referer, WebHeaderCollection header, CookieCollection cookies, Encoding encoding, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Get(url, referer, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.89 Safari/537.36", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8", 30000, header, cookies, encoding, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送Get请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="referer">参照页</param>
		/// <param name="cookies">Cookies</param>
		/// <param name="encoding">文本编码</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Get(string url, string referer, CookieCollection cookies, Encoding encoding, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Get(url, referer, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.89 Safari/537.36", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8", 30000, null, cookies, encoding, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送Get请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="referer">参照页</param>
		/// <param name="cookies">Cookies</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Get(string url, string referer, CookieCollection cookies, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Get(url, referer, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.89 Safari/537.36", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8", 30000, null, cookies, Encoding.UTF8, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送Get请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="referer">参照页</param>
		/// <param name="encoding">文本编码</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Get(string url, string referer, Encoding encoding, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Get(url, referer, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.89 Safari/537.36", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8", 30000, null, null, encoding, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送Get请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="referer">参照页</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Get(string url, string referer, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Get(url, referer, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.89 Safari/537.36", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8", 30000, null, null, Encoding.UTF8, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送Get请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Get(string url, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Get(url, "", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.89 Safari/537.36", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8", 30000, null, null, Encoding.UTF8, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送Post请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="parameters">请求参数</param>
		/// <param name="referer">参照页</param>
		/// <param name="userAgent">User-Agent HTTP标头</param>
		/// <param name="accept">Accept HTTP标头</param>
		/// <param name="timeout">超时时间</param>
		/// <param name="header">HTTP 标头</param>
		/// <param name="cookies">Cookies</param>
		/// <param name="encoding">文本编码</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Post(string url, Dictionary<string, string> parameters, string referer, string userAgent, string accept, int timeout, WebHeaderCollection header, CookieCollection cookies, Encoding encoding, DecompressionMethods decompression = DecompressionMethods.None)
		{
			string result = string.Empty;
			try
			{
				HttpWebRequest httpWebRequest = null;
				if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
				{
					ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
					httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
					httpWebRequest.ProtocolVersion = HttpVersion.Version10;
				}
				else
				{
					httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
				}
				httpWebRequest.UserAgent = userAgent;
				httpWebRequest.Referer = referer;
				httpWebRequest.Method = "POST";
				httpWebRequest.Timeout = timeout;
				httpWebRequest.Accept = accept;
				if (cookies != null)
				{
					httpWebRequest.CookieContainer = new CookieContainer();
					httpWebRequest.CookieContainer.Add(cookies);
				}
				if (header != null)
				{
					httpWebRequest.Headers = header;
				}
				httpWebRequest.AutomaticDecompression = decompression;
				httpWebRequest.ContentType = "application/x-www-form-urlencoded";
				if (parameters != null && parameters.Count > 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					int num = 0;
					foreach (string key in parameters.Keys)
					{
						stringBuilder.AppendFormat("{0}={1}", key, parameters[key]);
						if (num != parameters.Keys.Count - 1)
						{
							stringBuilder.Append("&");
						}
						num++;
					}
					httpWebRequest.ContentLength = stringBuilder.ToString().Length;
					byte[] bytes = Encoding.ASCII.GetBytes(stringBuilder.ToString());
					using (Stream stream = httpWebRequest.GetRequestStream())
					{
						stream.Write(bytes, 0, bytes.Length);
					}
				}
				HttpWebResponse webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				result = GetResponseString(webResponse, encoding);
				return result;
			}
			catch
			{
				return result;
			}
		}

		/// <summary>
		/// 向HTTP服务器发送Post请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="parameters">请求参数</param>
		/// <param name="referer">参照页</param>
		/// <param name="header">HTTP 标头</param>
		/// <param name="cookies">Cookies</param>
		/// <param name="encoding">文本编码</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Post(string url, Dictionary<string, string> parameters, string referer, WebHeaderCollection header, CookieCollection cookies, Encoding encoding, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Post(url, parameters, referer, "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8", 30000, header, cookies, encoding, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送Post请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="parameters">请求参数</param>
		/// <param name="referer">参照页</param>
		/// <param name="cookies">Cookies</param>
		/// <param name="encoding">文本编码</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Post(string url, Dictionary<string, string> parameters, string referer, CookieCollection cookies, Encoding encoding, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Post(url, parameters, referer, "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8", 30000, null, cookies, encoding, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送Post请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="parameters">请求参数</param>
		/// <param name="referer">参照页</param>
		/// <param name="cookies">Cookies</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Post(string url, Dictionary<string, string> parameters, string referer, CookieCollection cookies, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Post(url, parameters, referer, "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8", 30000, null, cookies, Encoding.UTF8, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送Post请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="parameters">请求参数</param>
		/// <param name="referer">参照页</param>
		/// <param name="encoding">文本编码</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Post(string url, Dictionary<string, string> parameters, string referer, Encoding encoding, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Post(url, parameters, referer, "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8", 30000, null, null, encoding, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送Post请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="parameters">请求参数</param>
		/// <param name="referer">参照页</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Post(string url, Dictionary<string, string> parameters, string referer, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Post(url, parameters, referer, "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8", 30000, null, null, Encoding.UTF8, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送Post请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="parameters">请求参数</param>
		/// <param name="decompression">加密方式</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string Post(string url, Dictionary<string, string> parameters, DecompressionMethods decompression = DecompressionMethods.None)
		{
			return Post(url, parameters, "", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8", 30000, null, null, Encoding.UTF8, decompression);
		}

		/// <summary>
		/// 向HTTP服务器发送请求, 获取 byte[]
		/// </summary>
		/// <param name="url">Url地址</param>
		/// <returns></returns>
		[Obsolete("请使用 HttpWebClient")]
		public static byte[] GetData(string url)
		{
			byte[] result = null;
			try
			{
				WebClient webClient = new WebClient();
				webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
				Stream stream = webClient.OpenRead(url);
				result = new byte[stream.Length];
				stream.Read(result, 0, result.Length);
				stream.Close();
				webClient = null;
				return result;
			}
			catch
			{
				return result;
			}
		}

		/// <summary>
		/// Url编码数据
		/// </summary>
		/// <param name="data">要编码的数据</param>
		/// <returns>编码后的数据</returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string UrlEncode(string data)
		{
			return HttpUtility.UrlEncode(data);
		}

		/// <summary>
		/// Url解码
		/// </summary>
		/// <param name="data">要解码的数据</param>
		/// <returns>解码后的数据</returns>
		[Obsolete("请使用 HttpWebClient")]
		public static string UrlDecode(string data)
		{
			return HttpUtility.UrlDecode(data);
		}

		/// <summary>
		/// 获取请求的数据
		/// </summary>
		/// <param name="webResponse"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		private static string GetResponseString(HttpWebResponse webResponse, Encoding encoding)
		{
			if (webResponse.ContentEncoding.ToLower().Contains("gzip"))
			{
				using (GZipStream stream = new GZipStream(webResponse.GetResponseStream(), CompressionMode.Decompress))
				{
					using (StreamReader streamReader = new StreamReader(stream, encoding))
					{
						return streamReader.ReadToEnd();
					}
				}
			}
			if (webResponse.ContentEncoding.ToLower().Contains("deflate"))
			{
				using (DeflateStream stream2 = new DeflateStream(webResponse.GetResponseStream(), CompressionMode.Decompress))
				{
					using (StreamReader streamReader2 = new StreamReader(stream2, encoding))
					{
						return streamReader2.ReadToEnd();
					}
				}
			}
			using (Stream stream3 = webResponse.GetResponseStream())
			{
				StreamReader streamReader3 = new StreamReader(stream3, encoding);
				return streamReader3.ReadToEnd();
			}
		}

		/// <summary>
		/// 验证证书
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="certificate"></param>
		/// <param name="chain"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			return true;
		}
	}
}
