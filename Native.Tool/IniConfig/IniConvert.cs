using Native.Tool.IniConfig.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Native.Tool.IniConfig
{
	/// <summary>
	/// 提供了普通对象针对 IniConfig 快速转换的类
	/// </summary>
	public static class IniConvert
	{
		/// <summary>
		/// 序列化多个对象为 <see cref="IniSection"/> 并组成 <see cref="IniObject"/>
		/// </summary>
		/// <param name="objs">需要序列化的对象, 该参数可以重复添加</param>
		/// /// <exception cref="ArgumentNullException">参数: objs 中有元素为 null</exception>
		/// <returns>序列化后返回 <see cref="IniObject"/> 对象</returns>
		public static IniObject SerializeObjects (params object[] objs)
		{
			IniObject iniobj = new IniObject (objs.Length);
			for (int i = 0; i < objs.Length; i++)
			{
				try
				{
					iniobj.Add (SerializeObject (objs[i]));
				}
				catch (ArgumentNullException ex)
				{
					throw new ArgumentNullException ("无法将 null 进行序列化", ex);
				}
			}
			return iniobj;
		}

		/// <summary>
		/// 序列化多个对象为 <see cref="IniSection"/> 并组成 <see cref="IniObject"/>
		/// </summary>
		/// <param name="dict">需要序列化的对象, 其对象的节点名为对应的 key, 该参数可以重复添加</param>
		/// <exception cref="ArgumentException">无法将空的节名用于Ini键值对</exception>
		/// <exception cref="ArgumentNullException">无法将 null 进行序列化</exception>
		/// <returns>序列化后返回 <see cref="IniObject"/> 对象</returns>
		public static IniObject SerializeObjects (IDictionary<string, object> dict)
		{
			IniObject iniobj = new IniObject (dict.Count);
			foreach (KeyValuePair<string, object> item in dict)
			{
				try
				{
					iniobj.Add (SerializeObject (item.Key, item.Value));
				}
				catch (ArgumentNullException ex)
				{
					throw new ArgumentNullException ("无法将 null 进行序列化", ex);
				}
				catch (ArgumentException ex)
				{
					throw new ArgumentException ("无法将空的节名用于Ini键值对", ex);
				}
			}
			return iniobj;
		}

		/// <summary>
		/// 序列化对象为 <see cref="IniSection"/>
		/// </summary>
		/// <param name="obj">需要序列化的对象</param>
		/// <exception cref="ArgumentException">无法将空的节名用于Ini键值对</exception>
		/// <exception cref="ArgumentNullException">参数: obj 为 null</exception>
		/// <returns>序列化后返回 <see cref="IniSection"/> 对象</returns>
		public static IniSection SerializeObject (object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException ("obj");
			}

			Type objType = obj.GetType ();
			IniSectionAttribute sectionAttribute = objType.GetCustomAttribute<IniSectionAttribute> ();
			if (sectionAttribute != null)
			{
				return SerializeObject (sectionAttribute.SectionName, obj);
			}
			else
			{
				return SerializeObject (objType.Name, obj);
			}
		}

		/// <summary>
		/// 序列化对象为 <see cref="IniSection"/>
		/// </summary>
		/// <param name="sectionName">返回结果使用的节名称</param>
		/// <param name="obj">需要序列化的对象</param>
		/// <exception cref="ArgumentException">无法将空的节名用于Ini键值对</exception>
		/// <exception cref="ArgumentNullException">参数: obj 为 null</exception>
		/// <returns>序列化后返回 <see cref="IniSection"/> 对象</returns>
		public static IniSection SerializeObject (string sectionName, object obj)
		{
			if (string.IsNullOrEmpty (sectionName))
			{
				throw new ArgumentException ("无法将空的节名用于Ini键值对", "sectionName");
			}

			if (obj == null)
			{
				throw new ArgumentNullException ("obj");
			}

			IniSection section = new IniSection (sectionName);

			// 获取所有属性
			IEnumerable<PropertyInfo> propertyInfos = obj.GetType ().GetRuntimeProperties ();
			foreach (PropertyInfo property in propertyInfos)
			{
				// 判断是否跳过序列化
				if (property.GetCustomAttribute<IniNonSerializeAttribute> () != null)
				{
					continue;
				}

				// 获取属性的值
				object iniValue = property.GetMethod.Invoke (obj, null);

				// 如果有标记, 使用标记作为 Key
				IniKeyAttribute keyAttribute = property.GetCustomAttribute<IniKeyAttribute> (); // 获取属性上的标记
				if (keyAttribute != null)
				{
					section.Add (keyAttribute.KeyName, Convert.ToString (iniValue));
				}
				else
				{
					section.Add (property.Name, Convert.ToString (iniValue));
				}
			}

			return section;
		}

		/// <summary>
		/// 反序列化 <see cref="IniSection"/> 为某个实例对象
		/// </summary>
		/// <typeparam name="T">反序列化的结果类型</typeparam>
		/// <param name="section">需要反序列化为对象的 <see cref="IniSection"/></param>
		/// <exception cref="ArgumentNullException">参数: section 为 null</exception>
		/// <returns>返回内容与 section 一致的实例</returns>
		public static T DeserializeObject<T> (IniSection section)
		{
			try
			{
				return (T)DeserializeObject (section, typeof (T));
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		/// 反序列化 <see cref="IniSection"/> 为某个实例对象
		/// </summary>
		/// <param name="section">需要反序列化为对象的 <see cref="IniSection"/></param>
		/// <param name="type">需要反序列化为的类型</param>
		/// <exception cref="ArgumentNullException">参数: section 或 type 为 null</exception>
		/// <returns>返回内容与 section 一致的实例</returns>
		public static object DeserializeObject (IniSection section, Type type)
		{
			if (section == null)
			{
				throw new ArgumentNullException ("section");
			}

			if (type == null)
			{
				throw new ArgumentNullException ("type");
			}


			// 判断 section 中的节名称是否与类名或标记相同
			IniSectionAttribute sectionAttribute = type.GetCustomAttribute<IniSectionAttribute> ();
			if (sectionAttribute != null)
			{
				if (sectionAttribute.SectionName.CompareTo (section.Name) != 0)
				{
					return null;
				}
			}
			else
			{
				if (type.Name.CompareTo (section.Name) != 0)
				{
					return null;
				}
			}

			// 创建实例
			object instance = Activator.CreateInstance (type);
			foreach (PropertyInfo property in type.GetRuntimeProperties ())
			{
				IniKeyAttribute keyAttribute = property.GetCustomAttribute<IniKeyAttribute> ();
				string key = null;
				if (keyAttribute != null)
				{
					key = keyAttribute.KeyName;
				}
				else
				{
					key = property.Name;
				}

				if (section.ContainsKey (key))
				{
					object value = Convert.ChangeType (section[key].Value, property.PropertyType);
					property.GetSetMethod (true).Invoke (instance, new object[] { value });
				}
			}
			return instance;
		}
	}
}
