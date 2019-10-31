# Native.SDK 更新日志
> 2019年10月13日 版本: V3.4.1.1016

	1. 修复 音乐接口的bug

> 2019年10月13日 版本: V3.4.0.1013

	1. 新增 群禁言事件 (Type: 104), 已升级 LibExport.tt 为新版本
	2. 新增 获取群信息接口, 支持获取当前人数和最大人数
	3. 新增 获取好友列表接口
	4. 修复 一些不稳定因素

> 2019年09月15日 版本: V3.3.0.0915

	1. 转移 App 下的 Interface 和 EventArgs 到 Sdk 项目中
	2. 修复 LibExport.tt 中 Type=21 事件永远 id 为 1
	3. 升级 LibExport.tt 使所有事件参数的 Id 和 Name 都与 Json 中相同
	4. 修改 Native.Csharp.Sdk 下的部分类命名空间及命名.

> 2019年09月06日 版本: V3.2.1.0906
	
	1. 修复 CqApi 部分接口引用不明确
	2. 修复 CqApi.GetFile 接口文件名乱码

> 2019年09月04日 版本: V3.2.0.0904
	
	1. 更新 CqApi.GetCookies 接口, 由专项获取 Cookies
	2. 更新 CqApi.CqCode_Music 接口, 新增新版分享样式 (Pro)
	3. 优化 CqApi.GetCookies 接口, 支持直接获取 CookiesCollection 对象 (测试)

> 2019年08月15日 版本: V3.1.11.0815

	1. 修复 LibExport.tt 导致的 "群私聊消息事件和好友消息事件" 必须同时注册的 bug

> 2019年08月05日 版本: V3.1.10.0805

	1. 修改 Unity 目标版本为 .Net Framework 4.5 以兼容 Docket

> 2019年07月31日 版本: V3.1.9.0731

	1. 修复 一些接口的内存泄漏 (具体表现: 非法内存读取)
	2. 优化 HttpWebClient 对于 Https 安全验证的初始化方式 (自动获取当前运行版本的验证和)

> 2019年07月30日 版本: V3.1.8.0730

	1. 修复 转换 String(GB18030) -> IntPrt 时内存泄漏的问题 (具体表现: 非法内存读取)
	2. 优化 HttpWebClient 对于 Https 的安全验证方式, 现在为手动指定. (用 TLS1.3 需要 .Net 4.8 的支持)
	3. 优化 HttpWebClient 中 Post 和 Get 方法的 Https 安全验证为 .Net 4.5 的所有验证机制

> 2019年07月16日 版本: V3.1.7.0716

	1. 优化 HttpWebClient 对 HTTPS 的验证, 增加了 TLS 1.3

> 2019年07月11日 版本: V3.1.6.0711

	1. 修复 Native.Csharp.Tool 项目在 Release 方式下 SQLite 组件会报大量的错误

> 2019年07月10日 版本: V3.1.5.0710

	1. 修正 CqApi.SetFriendAddRequest 方法第三个参数的定义及注释
	2. 修正 CqApi.SendPraise 方法的逻辑, 将第二个参数的范围限制在 1-10 之间
	3. 优化 CqApi.GetQQInfo 方法的调用方式
	4. 优化 CqApi.GetMemberInfo 方法的调用方式
	5. 优化 CqApi.GetMemberList 方法的调用方式
	6. 优化 CqApi.GetGroupList 方法的调用方式
	7. 优化 Native.Csharp.Sdk 项目的注释内容

> 2019年07月05日 版本: V3.1.4.0705

	1. 修复 HttpWebClient.CookieCollection 为 null 时, 开启自动合并更新会报错

> 2019年06月27日 版本: V3.1.3.0627

	1. 修复 HttpWebClient 重复 POST 请求引发 Headers 设置错误异常
	2. 修正 Native.Csharp.Tool 项目版本号
	3. 修正 Native.Csharp.Sdk 项目版本号

> 2019年06月23日 版本: V3.1.2.0623

	1. 修复 CqApi.ReceiveRecord 传递格式不正确的问题

> 2019年06月19日 版本: V3.1.1.0619

	1. 优化 Native.Csharp.Sdk 项目对 酷Q on Docker(Wine) 的兼容性
	2. 优化 Native.Csharp.Tool 项目对 酷Q on Docker(Wine) 的兼容性
	3. 优化 Native.Csharp.Repair 的兼容性 感谢 @kotoneme[https://github.com/kotoneme]
	4. 修复 OtherExpand.ToDateTime 方法转换时间出错
	5. 修复 CqApi.GetMemberList 方法获取数据时运算出错

> 2019年06月15日 版本: V3.1.0.0615

	1. 新增 MenuExport.tt 模板, 用于解析项目 Json 文件中的回调函数
	2. 新增 StatusExport.tt 模板, 用于解析项目 Json 文件中的回调函数
	3. 重写 LibExport.tt 模板, 用于解析项目 Json 文件中的回调函数
	4. 重写 Interface 中的接口, 分离所有事件为单独接口
	5. 重写 Model 中的所有模型, 更改为 EventArgs , 并重写所有事件
	6. 优化 为 Common.cs 增加 App名称 (酷Q应用列表显示的名称)
	7. 优化 为 Common.cs 增加 App版本 (酷Q应用列表显示的版本)
	8. 优化 为所有事件参数类增加 Json 文件对应的 Id, Name, Type 参数
	9. 优化 预置 Json 解析 nuget 包 (tt 模板关键组件, 请勿删除)
	10. 优化 调整了 IOC 容器的注册方式
	11. 优化 调整了 IOC 容器的实例化方式
	12. 修复 一些零碎的已知问题

> 2019年06月07日 版本: V3.0.7.0607

	由于 酷Q 停止对 Windows XP/Vista 系统的支持, 所以 Native.SDK 将停止继续使用 .Net 4.0 
	并将此版本作为最终发布版归档处理, 下个版本开始仅对 .Net 4.5+ 更新

	1. 修复 悬浮窗数据转换错误 (由 Pack -> BinaryWriter)
	2. 优化 部分 Api 接口的数据处理效率 (由 UnPack -> BinaryReader)
	3. 优化 分离 Native.Csharp.Tool 项目, 使 SDK 更轻量
	4. 优化 调整 Native.Csharp.Tool 项目结构, 每个模块为一个根文件夹. 排除即可在编译时移除功能
	5. 优化 新增 HttpTool (位于 Native.Csharp.Tool.Http)
	6. 新增 SQLite 操作类 (不包含EF, 需要可自行添加), 完全移植自 System.Data.SQLite (.Net 4.0)

> 2019年05月25日 版本: V3.0.6.0525

	1. 修复 HttpWebClient 类在请求 Internet 资源时响应重定向的部分代码错误
	2. 优化 HttpWebClient 类可指定自动合并更新 Cookies 功能
	3. 优化 CqMsg 类代码运行流程, 更符合规范
	4. 优化 CqCode 类, 支持获取当前实例在原文中的位置
	5. 优化 CqCode 类, 支持获取当前实例的原始字符串

> 2019年05月21日 版本: V3.0.5.0521

	1. 修复 兼容组件导致WFP窗体加载会出现资源无法找到

> 2019年05月17日 版本: V3.0.4.0517

	1. 修复 附加进程调试加载符号库出错

> 2019年05月14日 版本: V3.0.3.0514

	1. 修复 Repair 兼容组件不能正确重定向 (但是旧版本要兼容必须先关闭 Costura 的重定向, LoadAtModuleInit="False")
	2. 关闭 Costura 的重定向功能, 且在 SDK 加载时自动初始化

> 2019年05月12日 版本: V3.0.2.0512

	1. 修复 Fody 不兼容 Visual Studio 导致编译不通过的问题

> 2019年05月12日 版本: V3.0.1.0512

	1. 修复 从 Github 拉取项目后导致编译失败
	2. 转换 Native.Csharp.json 文件为 UTF-8 编码

> 2019年05月10日 版本: V3.0.1.0510

	说明: 由于酷Q改动了应用加载机制, 将所有开发中的应用都迁移至 dev 文件夹下, 所以本次更新将针对此优化进行改动

    1. 修复 Native.Csharp.Repair 应用兼容组件运行时间不正确
    2. 新增 SDK 生成前自动检查是否存在 AppID 目录
    3. 新增 SDK 生成前自动清理旧版 app.dll app.json 文件
    4. 新增 SDK 生成后自动复制并重命名程序集为 app.dll 自动复制并重命名json文件为 app.json
    5. 新增 SDK 生成后自动清理原生成目录下的程序集和 json 文件

> 2019年05月04日 版本: V3.0.0.0504

	说明: 由于酷Q改动了应用机制, 因此升级时请务必保存代码, 进行代码迁移升级!
	注意: 本次升级相对于之前的版本应用间不兼容做出了修改, 但是其机制导致了与旧版不兼容, 请酌情升级!

    1. 修复 CqApi.AddFatalError 方法传递错误时可能引发酷Q堆栈错误
    2. 修复 AppDomain.UnhandledException 全局异常捕获失效, 现在支持定位到方法
    3. 优化 AppDemain.UnhandledException 全局异常捕获解析方式
    4. 优化 项目版本号, 统一为项目新增当前版本号以区分
    5. 优化 项目事件模型, 抽象 EventArgsBase 类作为公共抽象类
    6. 优化 CqMsg 类, 完善 CqCodeType 枚举
    7. 优化 CqMsg 类, 更改 CqCode.Content 为字典, 而非键值对集合
    8. 优化 调试机制, 根据 酷Q 应用机制变动而转变为附加调试
    9. 优化 IOC容器的注册方式
    10.新增 Native.Csharp.Repair 组件, 改组件为原 (@成音S) 大佬的 .Net 兼容组件
    11.移除 Event_AppMain.Initialize 方法, 保证应用加载效率

> 2019年04月09日 版本: V2.7.3

	1. 修复 CqMsg 类针对 VS2012 的兼容问题
	2. 修复 HttpWebClient 类在增加 Cookies 时, 参数 "{0}" 为空字符串的异常
	3. 新增 HttpWebClient 类属性 "KeepAlive", 允许指定 HttpWebClient 在做请求时是否建立持续型的 Internal 连接

> 2019年04月06日 版本: V2.7.2

	1. 优化 Native.Csharp.Sdk 项目的结构, 修改类: CqApi 的命名空间
	2. 新增 消息解析类: CqMsg
	
``` C#
// 使用方法如下, 例如在群消息接受方法中
public void ReceiveGroupMessage (object sender, GroupMessageEventArgs e)
{
	var parseResult = CqMsg.Parse (e.Msg);		// 使用消息解析
	List<CqCode> cqCodes =  parseResult.Contents;	// 获取消息中所有的 CQ码
	
	// 此时, 获取到的 cqCodes 中就包含此条消息所有的 CQ码
}
```

> 2019年03月12日 版本: V2.7.1

	1. 新增 Sex 枚举中未知性别, 值为 255
	2. 优化 IOC 容器在获取对象时, 默认拉取所有注入的对象, 简化消息类接口的注入流程.

> 2019年03月03日 版本: V2.7.0

	本次更新于响应 "酷Q" 官方 "易语言 SDK" 的迭代更新
	
	1. 新增 CqApi.ReceiveImage (用于获取消息中 "图片" 的绝对路径)
	2. 新增 CqApi.GetSendRecordSupport (用于获取 "是否支持发送语音", 即用于区别 Air 和 Pro 版本之间的区别)
	3. 新增 CqApi.GetSendImageSupport (用于获取 "是否支持发送图片", 即用于区别 Air 和 Pro 版本指间的区别)
	4. 优化 CqApi.ReceiveRecord 方法, 使其获取到的语音路径为绝对路径, 而非相对路径

> 2019年02月26日 版本: V2.6.4

	1. 默认注释 Event_GroupMessage 中 ReceiveGroupMessage 方法的部分代码, 防止因为机器人复读群消息而禁言

> 2019年02月20日 版本: V2.6.3

	1. 还原 Event_AppMain.Resolvebackcall 方法的执行, 防止偶尔获取不到注入的类

> 2019年02月20日 版本: V2.6.2

	1. 更新 Native.Chsarp 项目的部分注释
	2. 新增 Event_AppMain.Initialize 方法, 位于 "Native.Csharp.App.Event" 下, 用于当作本项目的初始化方法
	3. 优化 Event_AppMain.Resolvebackcall 方法的执行, 默认将依据接口注入的类型全部实例化并取出分发到事件上 

> 2019年02月16日 版本: V2.6.1

	1. 优化 FodyWeavers.xml 配置, 为其加上注释. 方便开发者使用
	2. 修复 IniValue 中 ToType 方法导致栈溢出的

> 2019年01月26日 版本: V2.6.0

	说明: 此次更新重构了 Native.Csharp 项目, 全面采用依赖注入框架, 提升 SDK 可扩展性、可移植性
	注意: 此次更新核心代码重构, 开发者若要升级请备份好代码后升级.
	
	1. 新增 Unity 依赖注入框架, 提升 SDK 可扩展性,、可移植性
	2. 新增 LibExport 文件模板, 使用方式: 右击 "LibExport.tt" -> 运行自定义工具
	3. 新增 AppID 半自动填写 (运行 LibExport 模板即可生成)
	4. 新增 Event_AppMain 类, 改类主要用于注册回调和分发事件
	5. 新增 IEvent_AppStatus 接口, 用于实现 "酷Q事件"
	6. 新增 IEvent_DiscussMessage 接口, 用于实现 "讨论组事件"
	7. 新增 IEvent_FriendMessage 接口, 用于实现 "好友事件"
	8. 新增 IEvent_GroupMessage 接口, 用于实现 "群事件"
	9. 新增 IEvent_OtherMessage 接口, 用于实现 "其它事件"
	10. 新增 Ievent_UserExpand 接口, 用于事件开发者自定义的事件
	11. 修复 LibExport 中 "EventSystemGroupMemberDecrease" 事件传递的 FormQQ 不正确的问题 
	12. 优化 Model.GroupMessageEventArgs, 在其中添加 IsAnonymousMsg 的变量用于判断消息是否属于匿名消息
	13. 优化 对公共语言的支持
	14. 优化 对于原有不合理的事件分类进行重新整合.

> 2019年01月24日 版本: V2.5.0

	1. 新增 Fody 从 1.6.2 -> 3.2.1, 支持整体框架从 .Net Framework 4.0 到 .Net Framework 4.6.1, 开发者可以自行升级.

> 2019年01月23日 版本: V2.4.2

	1. 新增 IniObject.Load() 方法在加载了文件之后保持文件路径, 修改结束后可直接 Save() 不传路径参数保存
	2. 修复 IniObject.ToString() 方法的在转换 IniValue 时可能出错
	3. 补充 IniSection.ToString() 方法, 可以直接把当前实例转换为字符串, 可以直接被 IniObject.Parse() 解析
	4. 针对之后要推出的 Ini 配置项 序列化与反序列化问题做出优化

> 2019年01月22日 版本: V2.4.1

	1. 重载 IniSection 类的索引器, 现在获取值时 key 不存在不会抛异常, 而是返回 IniValue.Empty, 设置值时 key 不存在会直接调用 Add 方法将 key 加入到内部集合
	2. 重载 IniObject 类的 string 索引器, 现在设置 "节" 时, 不存在节会直接调用 Add 方法将节加入到内部集合

> 2019年01月21日 版本: V2.4.0
	
	说明: 本次更新主要解决 Native.Csharp.UI 项目不会被载入的问题, 描述为:当有多个 Native 开发的插件项目同时被酷
	      Q载入时, 会导致所有的插件项目只载入第一个 Native.Csharp.UI ! 所以请已经使用的 Native.Csharp.UI 项目
	      的开发者, 将现有的 Native.Charp.UI 项目的命名空间修改为其它命名空间(包括项目内的所有 xaml, cs)
	
	1. 移除 Native.Csharp.UI 项目, 保证后续被开发的项目窗体不会有冲突.
	
> 2019年01月21日 版本: V2.3.7

	1. 新增 IniValue 类针对基础数据类型转换时返回指定默认值的方法.

> 2019年01月20日 版本: V2.3.6

	1. 修复 IniObject 类针对解析过程中, 遇到 Value 中包含 "=", 从而导致匹配到的 Key 和 Value 不正确的问题 

> 2019年01月19日 版本: V2.3.5

	1. 新增 HttpWebClient 类针对 .Net Framework 4.0 的 https 的完整验证协议 感谢 @ycxYI[https://github.com/ycxYI]

> 2019年01月14日 版本: V2.3.4

	1. 修改 导出给酷Q的回调函数, 消息参数类型为 IntPtr 
	2. 修复 获取 酷Q GB18030 字符串, 转码异常的问题
	3. 修改 IniObject 类的继承类由 List<T> 转换为 Dictionary<TKey, TValue>
	4. 新增 IniObject 类的 string 类型索引器
	5. 新增 IniObject 类的 Add(IniSection) 方法, 默认以 IniSection.Name 作为键
	6. 新增 IniObject 类的 ToArray() 方法, 将返回一个 IniSection[]
	7. 重载 IniObject 类的 Add(string, IniSection) 方法, 无效化 string 参数, 默认以 IniSection.Name 作为键

> 2019年01月14日 版本: V2.3.3
	
	1. 还原 酷Q消息回调部分的导出函数的字符串指针 IntPtr -> String 类型, 修复此问题导致 酷Q 直接闪退
	
> 2019年01月11日 版本: V2.3.2

	1. 修复 传递给 酷Q 的消息编码不正确导致的许多文字在 QQ 无法正常显示的问题 感谢 @kotoneme[https://github.com/kotoneme], @gif8512 酷Q论坛[https://cqp.cc/home.php?mod=space&uid=454408&do=profile&from=space]
	2. 修复 酷Q 传递给 SDK 的消息由于编码不正确可能导致的开发问题 感谢 @kotoneme[https://github.com/kotoneme], @gif8512 酷Q论坛[https://cqp.cc/home.php?mod=space&uid=454408&do=profile&from=space]

> 2019年01月08日 版本: V2.3.1

	1. 修改 "nameof()" 方法调用为其等效的字符串形式, 修复 VS2012 编译报错

> 2018年12月29日 版本: V2.3.0
	
	说明: 此次更新改动较大, 请开发者在升级时备份好之前的代码!!!
	
	1. 分离了 Sdk.Cqp 为单独一个项目, 提升可移植性
	2. 修改 Native.Csharp.Sdk.Cqp.Api 中的 "EnApi" 的类名称为 "CqApi"
	3. 修改 "CqApi" 对象的构造方式, 由 "单例对象" 换为 "实例对象"
	4. 新增 "IniObject", "IniSection", "IniValue" , 位于 Native.Csharp.Tool.IniConfig.Linq (专门用于 Ini 配置项的类, 此类已完全面向对象)
	5. 弃用 Native.Csharp.Tool 中的 "IniFile" 类, (该类还能使用但是不再提供后续更新)

> 2018年12月13日 版本: V2.1.0

	1. 修复 DllExport 可能编译出失效的问题
  	2. 修复 异常处理 在try后依旧会向酷Q报告当前代码错误的问题
	3. 修复 异常处理 返回消息格式错误
	4. 修复 WPF窗体 多次加载会导致酷Q奔溃的问题
	5. 修复 "有效时间" 转换不正确 感谢 @BackRunner[https://github.com/backrunner]
	5. 分离 Sdk.Cqp.Tool -> Native.Csharp.Tool 提升代码可移植性

> 2018年12月07日 版本: V2.0.1

  	1. 修复 获取群列表永远为 null 感谢 @kotoneme[https://github.com/kotoneme]
	2. 修复 非简体中文系统下获取的字符串为乱码问题 感谢 @kotoneme[https://github.com/kotoneme]

> 2018年12月06日 版本: V2.0.0
	
	1. 重构 插件框架代码
	2. 修复 多插件同时运行时 "AuthCode" 发生串应用问题
	3. 优化 代码编译流程, 减少资源文件合并次数, 提升代码编译速率
	4. 优化 插件开发流程
	5. 新增 酷Q插件调试功能, 同时支持 Air/Pro 版本

> 2018年12月05日 版本: V1.1.2

	1. 修复 HttpWebClient 问题

> 2018年12月05日 版本: V1.1.1

	1. 尝试修复多应用由于 "AuthCode" 内存地址重复的问题导致调用API时会串应用的问题
	2. 优化SDK加载速度

> 2018年12月04日 版本: V1.1.0

	1. 由于酷Q废弃了消息接收事件中的 "font" 参数, SDK已经将其移除
	2. 修复 HttpHelper 类 "GetData" 方法中抛出异常
	3. 新增 HttpWebClient 类
	4. 新增 PUT, DELETE 请求方式
	5. 新增 在任何请求方式下 Cookies 提交, 回传, 自动合并更新
	6. 新增 在任何请求方式下 Headers 提交, 回传
	7. 新增 在任何请求方式下可以传入用于代理请求的 WebProxy 对象
	8. 新增 在任何请求方式下可以控制是否跟随响应 HTTP 服务器的重定向请求, 以及应和重定向次数
	9. 新增 可控制 "POST" 请求方式下的 "Content-Type" 标头的控制, 达到最大兼容性

> 2018年11月30日 版本: V1.0.0

	1. 打包上传项目
