## 简介

为了方便 **.Net** 平台开发者高效开发 **酷Q应用** , **Native.SDK** 所有事件都是面向接口完成的. 同时配合 Unity 依赖注入框架进行使用. 开发者不再需要关系 SDK 底层是如何将酷Q的事件转发给应用层, 只需要实现接口进行注册即可.

## 开发规范

* 事件相关的功能代码必须放入单独的项目中, 以 **AppID.Code** 命名
* 所有在事件中使用到的第三方依赖库必须同时安装到 **Native.Core** 项目中

## 使用 C#/VB.NET 实现接口

* 在解决方案中, 新建一个类库项目 (VB.NET项目类似), 项目名称以 **AppID.Code** 命名

    ![创建新项目](https://jie2gg.github.io/Image/Native.Framework/Native_CreateEventProject.png)

* 为 **AppID.Code** 项目添加 "Native.Sdk" 的引用, 同时在 "Native.Core" 中添加 **AppID.Code** 的引用

    ![引用添加效果](https://jie2gg.github.io/Image/Native.Framework/Native_AddUsing.png)

* 在 **AppID.Code** 项目中新建一个 "公开" 的类: "Event_GroupMessage", 该类继承 "IGroupMessage" 接口, 添加代码实现往来源群里复读消息

    * 在C#中使用如下代码:

    	```C#
        using Native.Sdk.Cqp.EventArgs;
        using Native.Sdk.Cqp.Interface;
        using Native.Sdk.Cqp.Model;

        namespace com.jiegg.demo.Code
        {
            public class Event_GroupMessage : IGroupMessage
            {
                /// <summary>
                /// 收到群消息
                /// </summary>
                /// <param name="sender">事件来源</param>
                /// <param name="e">事件参数</param>
                public void GroupMessage (object sender, CQGroupMessageEventArgs e)
                {
                    // 获取 At 某人对象
                    CQCode cqat = e.FromQQ.CQCode_At ();
                    // 往来源群发送一条群消息, 下列对象会合并成一个字符串发送
                    e.FromGroup.SendGroupMessage (cqat, " 您发送了一条消息: ", e.Message);
                    // 设置该属性, 表示阻塞本条消息, 该属性会在方法结束后传递给酷Q
                    e.Handler = true;
                }
            }
        }
    	```
    * 在VB中使用如下代码:    

    	```VB
        Imports Native.Sdk.Cqp.Interface
        Imports Native.Sdk.Cqp.EventArgs
        Imports Native.Sdk.Cqp.Model

        Namespace com.jiegg.demo.Code
            Public Class Event_GroupMessage
                Implements IGroupMessage
                ''' <summary>
                ''' 收到群消息
                ''' </summary>
                ''' <param name="sender">事件来源</param>
                ''' <param name="e">事件参数</param>
                Public Sub GroupMessage(sender As Object, e As CQGroupMessageEventArgs) Implements IGroupMessage.GroupMessage
                    ' 获取 At 某人对象
                    Dim cqat As CQCode = e.FromQQ.CQCode_At()
                    ' 往来源群发送一条群消息, 下列对象会合并成一个字符串发送
                    e.FromGroup.SendGroupMessage(cqat, " 您发送了一条消息: ", e.Message)
                    ' 设置该属性, 表示阻塞本条消息, 该属性会在方法结束后传递给酷Q
                    e.Handler = True
                End Sub
            End Class
        End Namespace
    	```

* 在 **CQMain** 中注册已实现的类, 该类位于 "Native.Core" -> "CQMain.cs"

    ```C#
    using Native.Sdk.Cqp.Interface;
    using {AppID}.Code;    // 这里的 {AppID} 替换成你决定的 AppID

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
        }
    }
    ```