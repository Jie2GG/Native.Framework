using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

//Name
[assembly: AssemblyTitle("酷Q样例应用 for C#")]
//Description
[assembly: AssemblyDescription("酷Q样例应用 for C# (V9应用机制)")]
//Author
[assembly: AssemblyCompany("JieGG")]
//AppId
[assembly: AssemblyProduct("native.csharp.app")]
//Version
[assembly: AssemblyVersion("1.0.0.2")]

[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: Guid("8bc2c861-d30c-425b-98a7-812792d2017d")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCopyright("Copyright ©  2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

[assembly: CoolQEventPrivateMsg("私聊消息处理", 30000)]
[assembly: CoolQEventGroupMsg("群消息处理", 30000)]
[assembly: CoolQEventDiscussMsg("讨论组消息处理", 30000)]
[assembly: CoolQEventGroupUpload("群文件上传事件处理", 30000)]
[assembly: CoolQEventGroupAdmin("群管理变动事件处理", 30000)]
[assembly: CoolQEventGroupMemberDecrease("群成员减少事件处理", 30000)]
[assembly: CoolQEventGroupMemberIncrease("群成员增加事件处理", 30000)]
[assembly: CoolQEventFriendAdded("好友已添加事件处理", 30000)]
[assembly: CoolQEventFriendRequest("好友添加请求处理", 30000)]
[assembly: CoolQEventGroupRequest("群添加请求处理", 30000)]
[assembly: CoolQEventStartup("酷Q启动事件", 30000)]
[assembly: CoolQEventExit("酷Q关闭事件", 30000)]
[assembly: CoolQEventEnable("应用已被启用", 30000)]
[assembly: CoolQEventDisable("应用将被停用", 30000)]
