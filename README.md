## Native.SDK 优点介绍

> 1. 程序集脱库打包
> 2. 原生c#开发体验
> 3. 完美翻译酷QApi
> 4. 支持酷Q应用打包
> 5. 支持代码实时调试

## Native.SDK 开发环境

>1. Visual Studio 2012 或更高版本
>2. Microsoft .Net Framework 4.0 **(XP系统支持的最后一个版本)**

## Native.SDK 开发流程

	1. 下载并打开 Native.SDK
	2. 打开 Native.Csharp 项目属性, 修改 "应用程序" 中的 "程序集名称" 为你的AppId (具体参见 http://d.cqp.me/Pro/开发/基础信息)
	3. 展开 Native.Csharp 项目, 修改 "Native.Csharp.json" 文件名为你的AppId
	4. 打开 Native.Csharp -> Event 文件夹, 修改 "Event_AppInitialize.cs" 中 "AppInfo" 方法的 "e.AppId" 的值为你的AppId
	
	此时 Native.SDK 的开发环境已经配置成功!

## Native.SDK 调试流程

	1. 打开菜单 生成 -> 配置管理器, 修改 "Native.Csharp" 项目的生成方式为 "Debug x86" 生成方式
	2. 打开项目 Native.Csharp 项目属性, 修改 "生成" 中的 "输出路径" 至酷Q的 "app" 目录
	3. 修改 "调试" 中的 "启动操作" 为 "启动外部程序", 并且定位到酷Q主程序
	4. 打开菜单 工具 -> 选项 -> 调试, 关闭 "要求源文件与原始版本匹配" 选项
	
	此时 Native.SDK 已经可以进行实时调试!

## Native.SDK 常见问题

  > 1. Native.Csharp 中的引用 DllExport 出现警告 <br />![警告](https://raw.githubusercontent.com/Jie2GG/Image_Folder/master/%E9%97%AE%E9%A2%98%E6%88%AA%E5%9B%BE.png "警告") <br/>

  * 打开解决方案根目录 <br /> ![Native根目录](https://raw.githubusercontent.com/Jie2GG/Image_Folder/master/Nativc%E6%A0%B9%E7%9B%AE%E5%BD%95.png "解决方案根路径")
  * 打开脚本文件 "DllExport_Configure.bat" <br /> ![打开脚本](https://raw.githubusercontent.com/Jie2GG/Image_Folder/master/Nativc%E8%BF%90%E8%A1%8C%E8%84%9A%E6%9C%AC.png "打开脚本")
  * 查看是否选中 Native.Csharp 项目, 并选择应用 <br /> ![应用项目](https://raw.githubusercontent.com/Jie2GG/Image_Folder/master/%E5%BA%94%E7%94%A8DllExport.png "应用项目")
  * 选择 "全部重新加载" <br /> ![重写加载](https://raw.githubusercontent.com/Jie2GG/Image_Folder/master/%E9%97%AE%E9%A2%98%E8%A7%A3%E5%86%B3.png "重新加载")

## Native.SDK 已知问题
	
> 1. ~~对于 "EnApi.GetMsgFont" 方法, 暂时无法根据酷Q回传的指针获取字体信息, 暂时无法使用~~ <font color=#FF0000>(由于酷Q不解析此参数, 弃用)</font>
> 2. ~~对于 "HttpHelper.GetData" 方法, 抛出异常, 暂时无法使用~~ <font color=#FF0000>(已经修复, 但是封装了新的HTTP类, 弃用)</font>
> 3. ~~对于 "AuthCode" 被多插件共用, 导致应用之间串数据~~ <font color=#FF0000>(已修复)</font>

## Native.SDK 更新日志
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
