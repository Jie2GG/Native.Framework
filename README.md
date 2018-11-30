# Native.SDK _--最贴近酷Q的 C# SDK_

### Native.SDK 优点介绍
> 1. 程序集脱库打包
> 2. 原生C#开发体验
> 3. 完美翻译酷QAPI

### Native.SDK 开发环境
> 1. Microsfot .Net Framework 4.0
> 2. Visual Studio 2012 或 更高版本

### Native.SDK 开发方式

    下载完毕后, Copy 一份Demo, 进行修改即可

### Native.SDK 项目结构
	
    ├─.vs
    ├─Native.Demo          (Demo例子, Copy后可以直接修改生成)
    │  ├─App               (存放Demo公共数据)
    │  ├─App.Core          (与酷Q交互部分)
    │  ├─App.Event         (酷Q事件回调)
    │  ├─App.Model         (所使用的模型类)
    │  ├─bin
    │  │  └─x86
    │  │      ├─Debug
    │  │      └─Release    (最终生成库文件路径)
    │  ├─obj
    │  └─Properties
    ├─Native.Sdk           (SDK, 封装了所有酷Q 原生API)
    │  ├─bin
    │  │  └─x86
    │  │      ├─Debug
    │  │      └─Release    (最终生成库文件路径)
    │  ├─Cqp               (存放SDK公共数据)
    │  ├─Cqp.Api           (对外公开的API)
    │  ├─Cqp.Core          (与酷Q交互部分, Api引用)
    │  ├─Cqp.Enum          (用到的枚举)
    │  ├─Cqp.Model         (所使用的模型类)
    │  ├─Cqp.Tool          (第三方工具类, 辅助开发用)
    │  ├─obj
    ├─Native.UI            (窗口UI, 使用MVVM构造应用所使用的后台窗口)
    │  ├─bin
    │  │  └─x86
    │  │      └─Release    (最终生成位置)
    │  ├─Models            (模型文件夹)
    │  ├─obj
    │  ├─Properties
    │  ├─ViewModels        (业务逻辑层文件夹)
    │  └─Views             (视图层文件夹)
    └─packages             (用到的包)

### Native.SDK 已知问题

  对于 "EnApi.GetMsgFont" 方法, 无法根据字体指针获取字体信息, 暂时无法使用. (待修复)

### Native.SDK 更新日志

> 2018年11月30日 V1.0.0
    
    打包上传项目
