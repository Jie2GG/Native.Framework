## Native.SDK 优点介绍

> 1. 程序集脱库打包
> 2. 类UWP开发体验
> 3. 完美翻译酷QApi
> 4. 支持酷Q应用打包
> 5. 支持附加进程调试

## Native.SDK 项目结构

![SDK结构](https://github.com/Jie2GG/Image/blob/master/NativeSDK(0).png "SDK结构") <br/>

## Native.SDK 开发环境

>1. Visual Studio 2012 或更高版本
>2. Microsoft .Net Framework 4.0 **(XP系统支持的最后一个版本)**

## Native.SDK 环境部署

	详情请看 Wiki: https://github.com/Jie2GG/Native.Csharp.Frame/wiki

## Native.SDK 调试流程

    1. 打开 酷Q Air/Pro, 并且登录机器人账号
    2. 打开 Native.Csharp 项目, 修改 "生成" 中的 "输出路径" 为 酷Q的 "dev" 路径
    3. 重新生成 Native.Csharp 项目
    4. 在酷Q上使用 "重载应用" 功能, 重载所有应用
    5. 依次选择VS的菜单项: "调试" -> "附加到进程"
    6. 选择 CQA.exe/CQP.exe 的托管进程, 选择附加
    7. 附加成功后进入调试模式, 即可进行断点 (注: 仅在只加载一个 .Net 应用的酷Q可以进行调试)

## Native.SDK 已知问题
	
> 1. ~~对于 "EnApi.GetMsgFont" 方法, 暂时无法根据酷Q回传的指针获取字体信息, 暂时无法使用~~ <span style="color:red">(由于酷Q不解析此参数, 弃用)</span>
> 2. ~~对于 "HttpHelper.GetData" 方法, 抛出异常, 暂时无法使用~~ <font color=#FF0000>(已经修复, 但是封装了新的HTTP类, 弃用)</font>
> 3. ~~对于 "AuthCode" 被多插件共用, 导致应用之间串数据~~ <font color=#FF0000>(已修复)</font>
> 4. ~~对于接收消息时, 颜文字表情, 特殊符号乱码, 当前正在寻找转换方式~~ <font color=#FF0000>(已修复)</font>
> 5. ~~对于 Visual Studio 弹出安全警告导致编译不通过的问题~~ <font color=#FF0000>(用 git 克隆到VS即可)</font>

## Native.SDK 更新日志

https://github.com/Jie2GG/Native.Csharp.Frame/blob/Final/UPDATE.md

## 关于打赏

您的支持就是我更新的动力!

<br/>
<img src="https://raw.githubusercontent.com/Jie2GG/Image/master/WeChat.png" width="260" height="350" alt="微信二维码"/>
<img src="https://raw.githubusercontent.com/Jie2GG/Image/master/AliPlay.png" width="260" height="350" alt="支付宝二维码"/>
