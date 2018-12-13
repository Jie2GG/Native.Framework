using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace System.Reflection
{
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventPrivateMsgAttribute : Attribute
	{
		public CoolQEventPrivateMsgAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventPrivateMsgAttribute(string name , int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "私聊消息处理";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventGroupMsgAttribute : Attribute
	{
		public CoolQEventGroupMsgAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventGroupMsgAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "群消息处理";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventDiscussMsgAttribute : Attribute
	{
		public CoolQEventDiscussMsgAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventDiscussMsgAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "讨论组消息处理";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventGroupUploadAttribute : Attribute
	{
		public CoolQEventGroupUploadAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventGroupUploadAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "群文件上传事件处理";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventGroupAdminAttribute : Attribute
	{
		public CoolQEventGroupAdminAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventGroupAdminAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "群管理变动事件处理";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventGroupMemberDecreaseAttribute : Attribute
	{
		public CoolQEventGroupMemberDecreaseAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventGroupMemberDecreaseAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "群成员减少事件处理";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventGroupMemberIncreaseAttribute : Attribute
	{
		public CoolQEventGroupMemberIncreaseAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventGroupMemberIncreaseAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "群成员增加事件处理";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventFriendRequestAttribute : Attribute
	{
		public CoolQEventFriendRequestAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventFriendRequestAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "好友添加请求处理";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventFriendAddedAttribute : Attribute
	{
		public CoolQEventFriendAddedAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventFriendAddedAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "好友已添加事件处理";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventGroupRequestAttribute : Attribute
	{
		public CoolQEventGroupRequestAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventGroupRequestAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "群添加请求处理";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventStartupAttribute : Attribute
	{
		public CoolQEventStartupAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventStartupAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "酷Q启动事件";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventExitAttribute : Attribute
	{
		public CoolQEventExitAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventExitAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "酷Q关闭事件";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventEnableAttribute : Attribute
	{
		public CoolQEventEnableAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventEnableAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "应用已被启用";
		public int Priority { get; } = 30000;
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class CoolQEventDisableAttribute : Attribute
	{
		public CoolQEventDisableAttribute(int priority)
		{
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public CoolQEventDisableAttribute(string name, int priority)
		{
			Name = name;
			Priority = (priority < 10000 || priority > 50000) ? 30000 : priority;
		}

		public string Name { get; } = "应用将被停用";
		public int Priority { get; } = 30000;
	}
}
