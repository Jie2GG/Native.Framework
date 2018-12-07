## Native.SDK 优点介绍

> 1. 程序集脱库打包
> 2. 原生c#开发体验
> 3. 完美翻译酷QApi
> 4. 支持酷Q应用打包
> 5. 支持代码直接调试

## Native.SDK 开发环境

>1. Visual Studio 2012 或更高版本
>2. Microsoft .Net Framework 4.0 **(XP系统支持的最后一个版本)**

## Native.SDK 开发流程

	1. 下载 Native.Sdk
	2. 打开 Native.Csharp 项目属性
	3. 修改 程序集名称中的 "Native.Csharp" 为你自己的 AppId (如: top.jiegg.demo.json)
	4. 修改 生成 -> 输出路径 为你的酷Q app 路径
	4. 找到 解决方案 -> Native.Csharp -> Native.Csharp.json
	5. 修改 文件名为你自己的应用ID (如: top.jiegg.demo.json)
	6. 开始编写酷Q插件 (默认: Release x86 编译方式)

## Native.SDK 调试流程

	1. 打开 Native.Csharp 项目属性
	2. 修改 生成 中的 "输出路径" 为你的 酷Q app 目录
	3. 修改 调试 -> 启动操作 中的 "启动外部程序"  为你的 酷Q 主程序, 之后保存
	4. 打开 工具 -> 选项 -> 调试, 关闭 "要求源文件与原始版本完全匹配" 项
	5. 修改 Native.Csharp 的生成选项为 Debug x86
	6. 开始调试你的 酷Q 程序

## Native.SDK 已知问题
	
> 1. ~~对于 "EnApi.GetMsgFont" 方法, 暂时无法根据酷Q回传的指针获取字体信息, 暂时无法使用~~ <font color=#FF0000>(由于酷Q不解析此参数, 弃用)</font>
> 2. ~~对于 "HttpHelper.GetData" 方法, 抛出异常, 暂时无法使用~~ <font color=#FF0000>(已经修复, 但是封装了新的HTTP类, 弃用)</font>
> 3. ~~对于 "AuthCode" 被多插件共用, 导致应用之间串数据~~ <font color=#FF0000>(已修复)</font>

## Native.SDK 更新日志
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