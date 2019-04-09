using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace Native.Csharp.Tool.Http
{
    /// <summary>
    /// HttpWebClient 操作类
    /// </summary>
    public class HttpWebClient : WebClient
    {
        #region --属性--
        /// <summary>
        /// 获取或设置请求的方法
        /// </summary>
        /// <exception cref="ArgumentException">未提供任何方法。 - 或 - 方法字符串包含无效字符。</exception>
        public string Method { get; set; }
        /// <summary>
        /// 获取或设置 User-Agent HTTP 标头的值
        /// </summary>
        public string UserAgent { get; set; }
        /// <summary>
        /// 获取或设置 Referer HTTP 标头的值
        /// </summary>
        public string Referer { get; set; }
        /// <summary>
        /// 获取或设置获取 Intelnet 资源过程的超时值
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">指定的值是小于零，且不是 System.Threading.Timeout.Infinite。</exception>
        public int TimeOut { get; set; }
        /// <summary>
        /// 获取或设置 Accept HTTP 标头的值
        /// </summary>
        public string Accept { get; set; }
        /// <summary>
        /// 获取或设置与此请求关联的 Cookie
        /// </summary>
        public CookieCollection CookieCollection { get; set; }
        /// <summary>
        /// 获取或设置 Content-Type HTTP 标头的值
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// 获取或设置一个值, 该值指示请求是否跟应跟随重定向响应
        /// </summary>
        public bool AllowAutoRedirect { get; set; }
        /// <summary>
        /// 获取或设置请求将跟随的重定向的最大数目。
        /// </summary>
        /// <exception cref="ArgumentException">值设置为 0 或更小。</exception>
        public int MaximumAutomaticRedirections { get; set; }
        /// <summary>
        /// 获取或设置一个值, 该值指示是否与 Internal 建立持续型的连接
        /// </summary>
        public bool KeepAlive { get; set; }
        #endregion

        #region --构造函数--
        /// <summary>
        /// 初始化 HttpWebClient 实例对象
        /// </summary>
        public HttpWebClient ()
        { }
        #endregion

        #region --公开方法--

        #region --Get--
        /// <summary>
        /// 向服务器发送 HTTP GET 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="userAgent">User-Agent HTTP 标头</param>
        /// <param name="accept">Accept HTTP 标头</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="proxy">代理 HttpWebClient 的 WebProxy 实例</param>
        /// <param name="encoding">文本编码</param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Get (string url, string referer, string userAgent, string accept, int timeout, ref CookieCollection cookies, ref WebHeaderCollection headers, WebProxy proxy, Encoding encoding, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            HttpWebClient httpWebClient = new HttpWebClient ();
            httpWebClient.CookieCollection = cookies;
            httpWebClient.Headers = headers;
            httpWebClient.Referer = referer;
            httpWebClient.UserAgent = userAgent;
            httpWebClient.Accept = accept;
            httpWebClient.TimeOut = timeout;
            httpWebClient.Encoding = encoding;
            httpWebClient.Proxy = proxy;
            httpWebClient.AllowAutoRedirect = allowAutoRedirect;
            byte[] result = httpWebClient.DownloadData (new Uri (url));
            headers = httpWebClient.ResponseHeaders;
            if (autoCookieMerge)
            {
                cookies = UpdateCookie (cookies, httpWebClient.CookieCollection);
            }
            else
            {
                cookies = httpWebClient.CookieCollection;
            }
            return result;
        }
        /// <summary>
        /// 向服务器发送 HTTP GET 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="proxy">代理 HttpWebClient 的 WebProxy 实例</param>
        /// <param name="encoding">文本编码</param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Get (string url, string referer, ref CookieCollection cookies, ref WebHeaderCollection headers, WebProxy proxy, Encoding encoding, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            return Get (url, referer, string.Empty, string.Empty, 0, ref cookies, ref headers, proxy, encoding, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP GET 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="encoding">文本编码</param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Get (string url, string referer, ref CookieCollection cookies, ref WebHeaderCollection headers, Encoding encoding, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            return Get (url, referer, ref cookies, ref headers, null, encoding, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP GET 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Get (string url, string referer, ref CookieCollection cookies, ref WebHeaderCollection headers, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            return Get (url, referer, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP GET 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        ///	</param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Get (string url, string referer, ref WebHeaderCollection headers, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            CookieCollection cookies = new CookieCollection ();
            return Get (url, referer, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP GET 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Get (string url, string referer, ref CookieCollection cookies, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            WebHeaderCollection headers = new WebHeaderCollection ();
            return Get (url, referer, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP GET 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        ///	</param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Get (string url, ref WebHeaderCollection headers, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            CookieCollection cookies = new CookieCollection ();
            return Get (url, string.Empty, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP GET 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Get (string url, ref CookieCollection cookies, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            WebHeaderCollection headers = new WebHeaderCollection ();
            return Get (url, string.Empty, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP GET 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Get (string url, string referer, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            WebHeaderCollection headers = new WebHeaderCollection ();
            CookieCollection cookies = new CookieCollection ();
            return Get (url, referer, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP GET 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Get (string url, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            return Get (url, string.Empty, allowAutoRedirect, autoCookieMerge);
        }
        #endregion

        #region --Post--
        /// <summary>
        /// 向服务器发送 HTTP POST 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="data">请求所需的上传数据</param>
        /// <param name="contentType">Content-Type HTTP 标头</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="userAgent">User-Agent HTTP 标头</param>
        /// <param name="accept">Accept HTTP 标头</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="proxy">代理 HttpWebClient 的 WebProxy 实例</param>
        /// <param name="encoding">文本编码</param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Post (string url, byte[] data, string contentType, string referer, string userAgent, string accept, int timeout, ref CookieCollection cookies, ref WebHeaderCollection headers, WebProxy proxy, Encoding encoding, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            HttpWebClient httpWebClient = new HttpWebClient ();
            httpWebClient.ContentType = contentType;
            httpWebClient.Referer = referer;
            httpWebClient.UserAgent = userAgent;
            httpWebClient.Accept = accept;
            httpWebClient.TimeOut = timeout;
            httpWebClient.CookieCollection = cookies;
            httpWebClient.Headers = headers;
            httpWebClient.Proxy = proxy;
            httpWebClient.AllowAutoRedirect = allowAutoRedirect;
            byte[] result = httpWebClient.UploadData (new Uri (url), data);
            headers = httpWebClient.ResponseHeaders;
            if (autoCookieMerge)
            {
                cookies = UpdateCookie (cookies, httpWebClient.CookieCollection);
            }
            else
            {
                cookies = httpWebClient.CookieCollection;
            }
            return result;
        }
        /// <summary>
        /// 向服务器发送 HTTP POST 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="data">请求所需的上传数据</param>
        /// <param name="contentType">Content-Type HTTP 标头</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="proxy">代理 HttpWebClient 的 WebProxy 实例</param>
        /// <param name="encoding">文本编码</param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Post (string url, byte[] data, string contentType, string referer, ref CookieCollection cookies, ref WebHeaderCollection headers, WebProxy proxy, Encoding encoding, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            return Post (url, data, contentType, referer, string.Empty, string.Empty, 0, ref cookies, ref headers, proxy, encoding, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP POST 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="data">请求所需的上传数据</param>
        /// <param name="contentType">Content-Type HTTP 标头</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="encoding">文本编码</param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Post (string url, byte[] data, string contentType, string referer, ref CookieCollection cookies, ref WebHeaderCollection headers, Encoding encoding, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            return Post (url, data, contentType, referer, ref cookies, ref headers, null, encoding, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP POST 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="data">请求所需的上传数据</param>
        /// <param name="contentType">Content-Type HTTP 标头</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Post (string url, byte[] data, string contentType, string referer, ref CookieCollection cookies, ref WebHeaderCollection headers, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            return Post (url, data, contentType, referer, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP POST 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="data">请求所需的上传数据</param>
        /// <param name="contentType">Content-Type HTTP 标头</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Post (string url, byte[] data, string contentType, string referer, ref WebHeaderCollection headers, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            CookieCollection cookies = new CookieCollection ();
            return Post (url, data, contentType, referer, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP POST 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="data">请求所需的上传数据</param>
        /// <param name="contentType">Content-Type HTTP 标头</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Post (string url, byte[] data, string contentType, string referer, ref CookieCollection cookies, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            WebHeaderCollection headers = new WebHeaderCollection ();
            return Post (url, data, contentType, referer, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP POST 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="data">请求所需的上传数据</param>
        /// <param name="contentType">Content-Type HTTP 标头</param>
        /// <param name="headers">请求附带的 Headers
        ///		<para>此参数支持自动更新 Headers</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Post (string url, byte[] data, string contentType, ref WebHeaderCollection headers, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            CookieCollection cookies = new CookieCollection ();
            return Post (url, data, contentType, string.Empty, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP POST 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="data">请求所需的上传数据</param>
        /// <param name="contentType">Content-Type HTTP 标头</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="cookies">请求附带的 Cookies
        ///		<para>此参数支持自动更新 Cookies, 若 cookieMerge 参数为True, 将合并新旧Cookie</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Post (string url, byte[] data, string contentType, ref CookieCollection cookies, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            WebHeaderCollection headers = new WebHeaderCollection ();
            return Post (url, data, contentType, string.Empty, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP POST 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="data">请求所需的上传数据</param>
        /// <param name="contentType">Content-Type HTTP 标头</param>
        /// <param name="referer">参考页链接
        ///		<para>告知服务器, 访问时的来源地址</para>
        /// </param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Post (string url, byte[] data, string contentType, string referer, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            WebHeaderCollection headers = new WebHeaderCollection ();
            CookieCollection cookies = new CookieCollection ();
            return Post (url, data, contentType, referer, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP POST 请求
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="data">请求所需的上传数据</param>
        /// <param name="contentType">Content-Type HTTP 标头</param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Post (string url, byte[] data, string contentType, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            WebHeaderCollection headers = new WebHeaderCollection ();
            CookieCollection cookies = new CookieCollection ();
            return Post (url, data, contentType, string.Empty, ref cookies, ref headers, null, Encoding.UTF8, allowAutoRedirect, autoCookieMerge);
        }
        /// <summary>
        /// 向服务器发送 HTTP POST 请求 (Conttent-Type 为 application/x-www-form-urlencoded)
        /// </summary>
        /// <param name="url">完整的网页地址
        ///		<para>必须包含 "http://" 或 "https://"</para>
        ///	</param>
        /// <param name="data">请求所需的上传数据</param>
        /// <param name="allowAutoRedirect">跟随重定向响应</param>
        /// <param name="autoCookieMerge">自动Cookie合并</param>
        /// <returns></returns>
        public static byte[] Post (string url, byte[] data, bool allowAutoRedirect = true, bool autoCookieMerge = true)
        {
            return Post (url, data, string.Empty, string.Empty, allowAutoRedirect, autoCookieMerge);
        }
        #endregion

        #region --Cookie--
        /// <summary>
        /// Cookie合并与更新
        /// </summary>
        /// <param name="oldCookies">原始的Cookis</param>
        /// <param name="newCookies">欲合并Cookies</param>
        /// <returns></returns>
        public static CookieCollection UpdateCookie (CookieCollection oldCookies, CookieCollection newCookies)
        {
            if (oldCookies == null)
            {
                throw new ArgumentNullException ("oldCookies");
            }
            if (newCookies == null)
            {
                throw new ArgumentNullException ("newCookies");
            }

            for (int i = 0; i < newCookies.Count; i++)
            {
                int index = CheckCookie (oldCookies, newCookies[i].Name);
                if (index >= 0)
                {
                    oldCookies[index].Value = newCookies[i].Value;
                }
                else
                {
                    oldCookies.Add (newCookies[i]);
                }
            }
            return oldCookies;
        }
        #endregion

        #region --URL--
        /// <summary>
        /// Url编码
        /// </summary>
        /// <param name="data">要编码的数据</param>
        /// <returns>编码后的数据</returns>
        public static string UrlEncode (string data)
        {
            return HttpUtility.UrlEncode (data);
        }

        /// <summary>
        /// Url解码
        /// </summary>
        /// <param name="data">要解码的数据</param>
        /// <returns>解码后的数据</returns>
        public static string UrlDecode (string data)
        {
            return HttpUtility.UrlDecode (data);
        }
        #endregion

        #endregion

        #region --私有方法--
        /// <summary>
        /// 验证HTTPS证书
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool CheckValidationResult (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        /// <summary>
        /// 确认Cookie是否存在
        /// </summary>
        /// <param name="cookie">Cookie对象</param>
        /// <param name="name">cookie名称</param>
        /// <returns></returns>
        private static int CheckCookie (CookieCollection cookie, string name)
        {
            for (int i = 0; i < cookie.Count; i++)
            {
                if (cookie[i].Name == name)
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion

        #region --重写方法--
        /// <summary>
        /// 返回带有 Cookies 的 HttpWebRequest
        /// </summary>
        /// <param name="address">一个 System.Uri ，它标识要请求的资源</param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest (Uri address)
        {
            if (address.OriginalString.StartsWith ("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;        // 强行验证HTTPS通过
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)(48 | 192 | 768 | 3072);   // 通过验证的协议类型, 来源 .Net Framework 4.5
            }
            HttpWebRequest httpWebRequest = (HttpWebRequest)base.GetWebRequest (address);
            httpWebRequest.ProtocolVersion = HttpVersion.Version11;
            httpWebRequest.KeepAlive = KeepAlive;   // 默认: False, 不建立持续型连接
            if (CookieCollection != null)
            {
                httpWebRequest.CookieContainer = new CookieContainer ();
                httpWebRequest.CookieContainer.Add (address, CookieCollection);
            }
            else
            {
                httpWebRequest.CookieContainer = new CookieContainer ();
            }
            if (!string.IsNullOrEmpty (this.UserAgent))
            {
                httpWebRequest.UserAgent = UserAgent;
            }
            else
            {
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.89 Safari/537.36";
            }
            if (TimeOut > 0)
            {
                httpWebRequest.Timeout = this.TimeOut;
            }
            if (!string.IsNullOrEmpty (this.Accept))
            {
                httpWebRequest.Accept = this.Accept;
            }
            else
            {
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            }
            if (this.AllowAutoRedirect)
            {
                httpWebRequest.AllowAutoRedirect = this.AllowAutoRedirect;
                if (this.MaximumAutomaticRedirections <= 0)
                {
                    httpWebRequest.MaximumAutomaticRedirections = 5;
                }
                else
                {
                    httpWebRequest.MaximumAutomaticRedirections = this.MaximumAutomaticRedirections;
                }
            }
            if (!string.IsNullOrEmpty (this.Referer))
            {
                httpWebRequest.Referer = this.Referer;
            }
            if (httpWebRequest.Method.ToUpper () != "GET")   //GET不需要包体参数
            {
                if (!string.IsNullOrEmpty (this.ContentType))
                {
                    httpWebRequest.ContentType = this.ContentType;
                }
                else
                {
                    httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                }
            }

            return httpWebRequest;
        }
        /// <summary>
        /// 返回指定 System.Net.WebResponse 的 System.Net.WebRequest。
        /// </summary>
        /// <param name="request">一个 System.Net.WebRequest 用于获得响应。</param>
        /// <returns>一个 System.Net.WebResponse 包含指定的响应 System.Net.WebRequest。</returns>
        protected override WebResponse GetWebResponse (WebRequest request)
        {
            HttpWebResponse httpWebResponse = (HttpWebResponse)base.GetWebResponse (request);
            this.CookieCollection = httpWebResponse.Cookies;
            this.Method = httpWebResponse.Method;
            this.ContentType = httpWebResponse.ContentType;
            return httpWebResponse;
        }
        #endregion
    }
}
