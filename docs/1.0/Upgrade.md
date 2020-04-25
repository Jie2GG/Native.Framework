## 简介

在 **Native.SDK** 4.0 之前的版本, 升级问题困扰了开发者很久. 从 4.0 版本开始, **Native.SDK** 优化了更新方式. 使开发者能更高效的进行版本迭代, 迅速更新. 

## 从老版本升级到 **4.0**

如果从 3.x 版本升级到 4.0, 建议开发者使用代码移植的方式. 由于 4.0 的变化较大. 3.x 版本的一些代码在 4.0 上会出现问题. 建议移植之后进行代码变动及优化操作.

## 从 4.0 升级到未来版本

* 下载最新的 **Native.SDK**
* 复制旧版本的 **AppID.Code** 和 **AppID.UI** 项目, 复制到新的SDK目录下 (如果有其它项目也一并复制)
* 打开解决方案, 将复制的项目添加到新的解决方案中, 并将 **AppID.Code** 和 **AppID.UI** 的引用添加到 **Native.Core** (如果有其它项目也一并引用)
* 为 **Native.Core** 安装在其它项目中包含的 nuget 包 (版本需一致)

    ![复制项目到新目录](https://jie2gg.github.io/Image/Native.Framework/Native_CopyProjectToNewSDK.png)
    ![添加项目引用](https://jie2gg.github.io/Image/Native.Framework/Native_AddProjectAndUsing.png)

* 按照 [快速入门](/Jie2GG/Native/wiki/01.-快速入门) 修改 **Native.SDK** 的程序集为 **AppID**
* 将原有项目的 **app.json** 覆盖新项目的 **app.json**, 并重新运行 CQExport.tt
* 将原有项目的 **CQMain.cs** 中注册的项目复制到新项目中
* 做完以上操作, 进行除错. 除错完毕即升级完成

```JSON
// 3. 将原来的 app.json 复制过来
{
    "ret": 1,
    "apiver": 9,
    "name": "复读机应用 for C#",
    "version": "1.0.0",
    "version_id": 1,
    "author": "Jie2GG",
    "description": "WIKI所使用的 Demo",
    "event": [
        {
            "id": 1,
            "type": 2,
            "name": "群消息处理",
            "function": "eventGroupMsg",
            "priority": 30000
        }
    ],
    "menu": [
        {
            "name": "打开Demo窗体",
            "function": "menuOpenWindow"
        }
    ],
    "status": [
        {
            "id": 1,
            "name": "统计发送",
            "title": "SEND",
            "function": "statusUpMsgCount",
            "period": "1000"
        }
    ],
    "auth": [
        101 //发送群消息
    ]
}
```

```C#
// 4. 将原有的注册项目复制过来
/// <summary>
/// 酷Q应用主入口类
/// </summary>
public class CQMain
{
    /// <summary>
    /// 在应用被加载时将调用此方法进行事件注册, 请在此方法里向 <see cref="IUnityContainer"/> 容器中注册需要使用的事件
    /// </summary>
    /// <param name="container">用于注册的 IOC 容器 </param>
    public static void Register (IUnityContainer unityContainer)
    {
        // 在 Json 中, 群消息的 name 字段是: 群消息处理, 因此这里注册的第一个参数也是这样填写
        unityContainer.RegisterType<IGroupMessage, Event_GroupMessage> ("群消息处理");

        // 在这里添加窗体菜单的类注册
        unityContainer.RegisterType<IMenuCall, Menu_OpenWindow> ("打开Demo窗体");

        // 在这里添加悬浮窗的类注册
        unityContainer.RegisterType<IStatusUpdate, Status_UpMsgCount> ("统计发送");
    }
}
```