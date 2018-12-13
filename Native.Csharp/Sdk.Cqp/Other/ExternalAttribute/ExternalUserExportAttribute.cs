using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class CoolQMenuAttribute : Attribute
	{
		public CoolQMenuAttribute(string name, string function)
		{
			Name = name;
			@Function = function;
		}
		public string Name { get; } = "打开控制台";
		public string @Function { get; } = "_eventOpenConsole";
	}
}
