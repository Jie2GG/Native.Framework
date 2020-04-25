## 简介

使用 Native.SDK 参与酷Q应用开发, 需要将 **酷Q** 设置到开发模式, 同时正确配置 SDK 的信息. 本章节将以 **酷Q Air** 和 **Native.SDK (4.1版)** 为例, 请按照以下步骤操作

## 环境准备

> 系统环境
* Microsoft Windows 7 或更高版本

> 软件环境
* Visual Studio 2012 或更高版本
* .Net Framework 4.5
* 酷Q Air/Pro

## 获取 **Native.SDK**

您可以在我的 Github 主页获取 Native.SDK 的软件开发工具包

* 主页地址: https://github.com/Jie2GG/Native.Framework
* 克隆地址: https://github.com/Jie2GG/Native.Framework.git

## 注意事项

`在使用 "Native.SDK" 时, 请务必不要改动附随 Nuget 扩展库的版本, 以免带来一系列的麻烦 !!!`<br/>
`在使用 "Native.SDK" 时, 请务必不要改动附随 Nuget 扩展库的版本, 以免带来一系列的麻烦 !!!`<br/>
`在使用 "Native.SDK" 时, 请务必不要改动附随 Nuget 扩展库的版本, 以免带来一系列的麻烦 !!!`<br/>

## 开发前准备

> 启用 **酷Q开发者模式**

* 右击 **酷Q悬浮窗**
* 选择 **应用** -> **应用管理**, 稍后将会弹出一个新窗体
* **5** 次点击窗体右下角的 "版本号", 之后将会弹出高级设置
* 勾选 **开发模式**、**应用模块隔离**, 点击 **重启酷Q** 按钮

开启开发者模式后, 酷Q将新增以下功能:
* 错误信息将详细显示
* 支持载入未打包的应用 (开发中的应用)

#### **`注意: .Net 应用开发必须启用 **应用模块隔离** 以提升兼容性.`**

![启用酷Q开发者模式](https://jie2gg.github.io/Image/Native.Framework/Native_EnableDev.png)

> 下载 **Native.SDK**

* `[推荐]`方法一:
    
    1. 打开 **Git CMD.exe**
    2. 定位到非C盘的路径, (如 D:\CoolQProject)
    3. 输入命令 "git clone https://github.com/Jie2GG/Native.Framework.git"

* 方法二:

    1. 在 Github 主页点击 **"Clone or Download"** 按钮
    2. 点击 **"Download ZIP"** 按钮
    3. 待下载完成后, 解压到非C盘路径(如 D:\CoolQProject)

## 设置 **AppID**

**AppID** 是应用的唯一标识符. 同一个应用的 **AppID** 长期不变, 不同的 **AppID** 不同, 具体规则参见 **[AppID规则](https://docs.cqp.im/dev/v9/appid/)**
<br/>
确定了该应用的 **AppID** 后, 请按照以下步骤设置 **Native.SDK**

* 打开 **Native.sln** 工程文件
* 在 **解决方案管理器** 中打开 **Native.Core** 项目的属性
* 修改项目的 **程序集名称** 为 **AppID**, 例如: com.jiegg.demo
* 选择 "Native.Core" -> "Export" -> "CQExport.tt" 文件, 右键选择 **运行自定义工具**

> 若在运行过程中, 没有发生任何错误与警告. 则表示 tt 文件编译正常. 至此 **AppID** 设置完成

![设置AppID](https://jie2gg.github.io/Image/Native.Framework/Native_SetAppID.png)
![运行自定义工具](https://jie2gg.github.io/Image/Native.Framework/Native_RunTool.png)