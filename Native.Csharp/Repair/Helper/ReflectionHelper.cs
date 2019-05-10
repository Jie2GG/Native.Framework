using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Repair
{
	/*
	 *	移植自: 00.00.dotnetRedirect 插件, 原作者: 成音S. 引用请带上此注释
	 *	论坛地址: https://cqp.cc/t/42920
	 */
	public static class ReflectionHelper
	{
		#region --字段--
		public static BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
		#endregion

		#region --公开方法--
		public static T InvokeMethod<T> (Type type, object instance, string methodName, params object[] args)
		{
			var method = GetMethod (type, methodName);
			if (method != null)
			{
				return (T)method.Invoke (instance, args);
			}
			return default (T);
		}

		public static T InvokeMethod<T> (this T obj, string methodName, params object[] args)
		{
			var type = typeof (T);
			var method = GetMethod (type, methodName);
			if (method != null)
			{
				return (T)method.Invoke (obj, args);
			}
			return default (T);
		}

		public static MethodInfo GetMethod (Type type, string methodName)
		{
			var result = type.GetMethods ().Where (mi => mi.Name == methodName).FirstOrDefault ();
			if (result != null)
			{
				return result;
			}
			return null;
		}

		public static T GetInstanceField<T> (Type type, object instance, string fieldName)
		{
			FieldInfo field = type.GetField (fieldName, bindFlags);
			return (T)field.GetValue (instance);
		}

		public static void SetInstanceField<T> (Type type, object instance, string fieldName, T fieldValue)
		{
			FieldInfo field = type.GetField (fieldName, bindFlags);
			field.SetValue (instance, fieldValue);
		}

		public static void ClearEventInvocations (this object obj, string eventName)
		{
			var fi = obj.GetType ().GetEventField (eventName);
			if (fi != null)
			{
				fi.SetValue (obj, null);
			}
		}

		public static FieldInfo GetEventField (this Type type, string eventName)
		{
			FieldInfo field = null;
			while (type != null)
			{
				field = type.GetField (eventName, bindFlags);
				if (field != null && (field.FieldType == typeof (MulticastDelegate) || field.FieldType.IsSubclassOf (typeof (MulticastDelegate))))
				{
					break;
				}

				field = type.GetField ("EVENT_" + eventName.ToUpper (), bindFlags);
				if (field != null)
				{
					break;
				}
				type = type.BaseType;
			}
			return field;
		}
		#endregion
	}
}
