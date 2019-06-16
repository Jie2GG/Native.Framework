using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Native.Csharp.Sdk.Cqp;
using Unity;

namespace Native.Csharp.App
{
	/// <summary>
	/// 用于存放 App 数据的公共类
	/// </summary>
	public static class Common
	{
        /// <summary>
        /// 获取 App 在酷Q中显示的名称 (此属性 set 并不能做出实质性的修改)
        /// </summary>
        public static string AppName { get; set; }

        /// <summary>
        /// 获取 App 在酷Q中的版本号 (此属性 set 并不能做出实质性的修改)
        /// </summary>
        public static Version AppVersion { get; set; }

        /// <summary>
		/// 获取或设置 App 在运行期间所使用的数据路径
		/// </summary>
		public static string AppDirectory { get; set; }

        /// <summary>
        /// 获取或设置当前 App 是否处于运行状态
        /// </summary>
        public static bool IsRunning { get; set; }

        /// <summary>
        /// 获取或设置当前 App 使用的依赖注入容器实例
        /// </summary>
        public static IUnityContainer UnityContainer { get; set; }
		
		/// <summary>
		/// 获取或设置当前 App 使用的 酷Q Api 接口实例
		/// </summary>
		public static CqApi CqApi { get; set; }
	}
}
