using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Native.Csharp.Sdk.IOC
{
	/// <summary>
	/// 表示 <see cref="global::Unity.UnityContainer"/> 的操作类
	/// </summary>
	public static class Unity
	{
		#region --字段--
		private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer> (() => { return new UnityContainer (); });
		#endregion

		#region --公开方法--
		//public static Unity RegisterType<T> (string name, params object[] parameterValues)
		//{
			
		//}
		#endregion
	}
}
