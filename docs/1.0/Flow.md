## 简介

酷Q提供了 **悬浮窗扩展** 功能, 我们可以利用扩展悬浮窗显示的内容来观察插件的运行状态, 如: 获取CPU使用率, 内存占用等需求. 

## 开发建议

* 悬浮窗的事件代码建议放在 **AppID.Code** 项目中, 方便统计事件运行的情况
* 所有在事件中使用到的第三方依赖库必须同时安装到 **Native.Core** 项目中

## 修改Json添加悬浮窗

请根据 [配置规范](/Jie2GG/Native.Framework/wiki/02.-配置Json) 按照如下展示为 app.json 配置一项悬浮窗

```JSON
"status": [
    {
        "id": 1, 
        "name": "统计发送",
        "title": "SEND",
        "function": "statusUpMsgCount",
        "period": "1000"
    }
]
```

### 注意: 配置完 Json 之后务必运行一次 **CQExport.tt** 以更新SDK的导出函数

## 在 C#/VB.NET 中实现悬浮窗事件

* 在 **AppID.Code** 项目中新建一个 "公共数据" 的类: "CommonData",  该类用来存储计算出的消息数量

    * 在C#中使用如下代码:

        ```C#
        namespace com.jiegg.demo.Code
        {
            public static class CommonData
            {
                private static readonly object lockobject = new object ();

                private static int _msgCount;
                /// <summary>
                /// 获取或设置消息条数
                /// </summary>
                public static int MsgCount
                {
                    get
                    {
                        lock (lockobject)
                        {
                            return _msgCount;
                        }
                    }
                    set
                    {
                        lock (lockobject)
                        {
                            _msgCount = value;
                        }
                    }
                }
            }
        }
        ```

    * 在VB中使用如下代码:

        ```VB
        Namespace com.jiegg.demo.Code
            Public Class CommonData
                Private Shared ReadOnly lockObject As Object = New Object()

                Private Shared _msgCount As Int32
                ''' <summary>
                ''' 获取或设置消息条数
                ''' </summary>
                Public Shared Property MsgCount() As Int32
                    Get
                        SyncLock (lockObject)
                            Return _msgCount
                        End SyncLock
                    End Get
                    Set(ByVal value As Int32)
                        SyncLock (lockObject)
                            _msgCount = value
                        End SyncLock
                    End Set
                End Property
            End Class
        End Namespace
        ```

* 修改 **AppID.Code** 项目的 "Event_GroupMessage.cs" (VB.NET项目类似), 添加消息计算代码

    * 在C#中使用如下代码:
        
        ```C#
        public class Event_GroupMessage : IGroupMessage
        {
            /// <summary>
            /// 收到群消息
            /// </summary>
            /// <param name="sender">事件来源</param>
            /// <param name="e">事件参数</param>
            public void GroupMessage (object sender, CQGroupMessageEventArgs e)
            {
                // 在原有的基础上增加消息统计
                CommonData.MsgCount += 1;

                // 获取 At 某人对象
                CQCode cqat = e.FromQQ.CQCode_At ();
                // 往来源群发送一条群消息, 下列对象会合并成一个字符串发送
                e.FromGroup.SendGroupMessage (cqat, " 您发送了一条消息: ", e.Message);
                // 设置该属性, 表示阻塞本条消息, 该属性会在方法结束后传递给酷Q
                e.Handler = true;
            }
        }
        ```
    
    * 在VB中使用如下代码:

        ```VB
        Public Class Event_GroupMessage
            Implements IGroupMessage
            ''' <summary>
            ''' 收到群消息
            ''' </summary>
            ''' <param name="sender">事件来源</param>
            ''' <param name="e">事件参数</param>
            Public Sub GroupMessage(sender As Object, e As CQGroupMessageEventArgs) Implements IGroupMessage.GroupMessage
                ' 在原有的基础上增加消息统计
                CommonData.MsgCount += 1

                ' 获取 At 某人对象
                Dim cqat As CQCode = e.FromQQ.CQCode_At()
                ' 往来源群发送一条群消息, 下列对象会合并成一个字符串发送
                e.FromGroup.SendGroupMessage(cqat, " 您发送了一条消息: ", e.Message)
                ' 设置该属性, 表示阻塞本条消息, 该属性会在方法结束后传递给酷Q
                e.Handler = True
            End Sub
        End Class
        ```

* 在 **AppID.Code** 项目中新建一个 "公开" 的类: "Status_Refresh", 该类继承 "IStatusUpdate" 接口, 添加代码实现更新悬浮窗数据

    * 在C#中使用如下代码:

        ```C#
        using Native.Sdk.Cqp.Enum;
        using Native.Sdk.Cqp.EventArgs;
        using Native.Sdk.Cqp.Interface;
        using Native.Sdk.Cqp.Model;

        namespace com.jiegg.demo.Code
        {
            public class Status_UpMsgCount : IStatusUpdate
            {
                /// <summary>
                /// 更新悬浮窗统计数据
                /// </summary>
                /// <param name="sender">事件来源</param>
                /// <param name="e">事件参数</param>
                public CQFloatWindow StatusUpdate (object sender, CQStatusUpdateEventArgs e)
                {
                    CQFloatWindow floatWindow = new CQFloatWindow ();
                    floatWindow.Value = CommonData.MsgCount;            // 设置展示值
                    floatWindow.Unit = "条";                            // 设置单位
                    floatWindow.TextColor = CQFloatWindowColors.Green;  // 设置文本颜色
                    return floatWindow;
                }
            }
        }
        ```

    * 在VB中使用如下代码:

        ```VB
        Imports Native.Sdk.Cqp.Enum
        Imports Native.Sdk.Cqp.EventArgs
        Imports Native.Sdk.Cqp.Interface
        Imports Native.Sdk.Cqp.Model

        Namespace com.jiegg.demo.Code
            Public Class Status_UpMsgCount
                Implements IStatusUpdate
                ''' <summary>
                ''' 更新悬浮窗统计数据
                ''' </summary>
                ''' <param name="sender">事件来源</param>
                ''' <param name="e">事件参数</param>
                ''' <returns>返回用于展示的 <see cref="CQFloatWindow"/> 对象</returns>
                Public Function StatusUpdate(sender As Object, e As CQStatusUpdateEventArgs) As CQFloatWindow Implements IStatusUpdate.StatusUpdate
                    Dim floatWindow As CQFloatWindow = New CQFloatWindow()
                    floatWindow.Value = CommonData.MsgCount             ' 设置展示值
                    floatWindow.Unit = "条"                             ' 设置单位
                    floatWindow.TextColor = CQFloatWindowColors.Green   ' 设置文本颜色
                    Return floatWindow
                End Function
            End Class
        End Namespace
        ```

* 在 **CQMain** 中注册已实现的类, 该类位于 "Native.Core" -> "CQMain.cs"

    ```C#
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

## 编译测试

根据 [编译部署](/Jie2GG/Native.Framework/wiki/04.-编译部署) 中的步骤, 将SDK编译, 重新部署到 "dev" 目录, 然后使用酷Q测试悬浮窗的加载

![效果展示](https://jie2gg.github.io/Image/Native.Framework/Native_ShowFloatWindow.png)

