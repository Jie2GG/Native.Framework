## Native.SDK 优点介绍

> 1. 程序集脱库打包
> 2. 原生c#开发体验
> 3. 完美翻译酷QApi
> 4. 支持库Q应用打包

## Native.SDK 开发环境

>1. Visual Studio 2012 或更高版本
>2. Microsoft .Net Framework 4.0 **(XP系统支持的最后一个版本)**

## Native.SDK 开发流程

	1. 下载 Native.SDK
	2. 打开 解决方案 -> Native.Demo -> App.Event -> Event_AppInitialize.cs
	3. 修改 "AppInfo" 方法中的 e.AppId 为你自己的应用ID
	4. 找到 解决方案 -> Native.Demo -> Native.Demo.json
	5. 修改 文件名为你自己的应用ID (如: top.jiegg.demo.json)
	6. 开始编写你的应用
	7. 生成 Native.Demo 项目, 成功后找到该项目的 bin -> x86 -> Release -> Native.Demo.dll
	8. 将dll文件名修改为你自己的应用ID (如: top.jiegg.demo.dll)
	9. 复制到 酷Q app 目录

## Native.SDK 已知问题
	
> 1. ~~对于 "EnApi.GetMsgFont" 方法, 暂时无法根据酷Q回传的指针获取字体信息, 暂时无法使用~~ (由于酷Q不解析此参数, 弃用)
> 2. ~~对于 "HttpHelper.GetData" 方法, 抛出异常, 暂时无法使用~~ (已经修复, 但是封装了新的HTTP类, 弃用)

## Native.SDK 更新日志
> 2018年12月01日 版本: V1.1.0

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