## 简介

很多情况, 需要给应用增加一个窗体. 本例将使用 .Net 平台广泛使用的 **WPF** 窗体, 将其添加到 **Native.SDK** 中, 让窗体以组件的形式在酷Q中以 **菜单** 进行创建.

## 开发规范

* 界面相关的功能代码必须放入单独的UI项目中, 以 **AppID.UI** 命名
* 所有在界面中使用到的第三方依赖库必须同时安装到 **Native.Core** 项目中

## 修改Json添加菜单

请根据 [配置规范](/Jie2GG/Native.Framework/wiki/02.-配置Json) 按照如下展示为 app.json 配置一项菜单

```JSON
"menu": [
    {
        "name": "打开Demo窗体",
        "function": "menuOpenWindow"
    }
]
```

### 注意: 配置完 Json 之后务必运行一次 **CQExport.tt** 以更新SDK的导出函数

## 在 C#/VB.NET 中实现菜单事件打开窗体

* 在解决方案中, 新建一个 **WPF** 项目(VB.NET项目类似), 项目名称以 **AppID.UI** 命名

    ![添加项目](https://jie2gg.github.io/Image/Native.Framework/Native_CreateUIProject.png)

* 为 **AppID.UI** 项目添加 "Native.Sdk" 的引用, 同时在 "Native.Core" 中添加 **AppID.UI** 的引用

    ![添加引用](https://jie2gg.github.io/Image/Native.Framework/Native_AddUsingforUI.png)

* 在 **AppID.UI** 项目中新建一个 "公开" 的类: "Menu_OpenWindow", 该类继承 "IMenuCall" 接口, 添加代码实现弹出窗体

    * 在C#中使用如下代码:

        ```C#
        using Native.Sdk.Cqp.EventArgs;
        using Native.Sdk.Cqp.Interface;

        namespace com.jiegg.demo.UI
        {
            public class Menu_OpenWindow : IMenuCall
            {
                private MainWindow _mainWindow = null;

                /// <summary>
                /// 打开窗体按钮被按下
                /// </summary>
                /// <param name="sender">事件来源</param>
                /// <param name="e">事件参数</param>
                public void MenuCall (object sender, CQMenuCallEventArgs e)
                {
                    if (this._mainWindow == null)
                    {
                        this._mainWindow = new MainWindow ();
                        this._mainWindow.Closing += MainWindow_Closing;
                        this._mainWindow.Show ();	// 显示窗体
                    }
                    else
                    {
                        this._mainWindow.Activate ();	// 将窗体调制到前台激活
                    }
                }

                private void MainWindow_Closing (object sender, System.ComponentModel.CancelEventArgs e)
                {
                    // 对变量置 null, 因为被关闭的窗口无法重复显示
                    this._mainWindow = null;
                }
            }
        }
        ```

    * 在VB中使用如下代码:

        ```VB
        Imports System.ComponentModel
        Imports Native.Sdk.Cqp.EventArgs
        Imports Native.Sdk.Cqp.Interface

        Namespace com.jiegg.demo.UI
            Public Class Menu_OpenWindow
                Implements IMenuCall

                Private _mainWindow As MainWindow = Nothing
                ''' <summary>
                ''' 收到群消息
                ''' </summary>
                ''' <param name="sender">事件来源</param>
                ''' <param name="e">事件参数</param>
                Public Sub MenuCall(sender As Object, e As CQMenuCallEventArgs) Implements IMenuCall.MenuCall
                    If _mainWindow Is Nothing Then
                        _mainWindow = New MainWindow()
                        AddHandler _mainWindow.Closing, AddressOf MainWindow_Closing
                        _mainWindow.Show()  ' 显示窗体
                    Else
                        _mainWindow.Activate()  ' 将窗体调制到前台激活
                    End If
                End Sub

                Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs)
                    ' 对变量置 Nothing, 因为被关闭的窗口无法重复显示
                    _mainWindow = Nothing
                End Sub
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
        }
    }
    ```

## 编译测试

根据 [编译部署](/Jie2GG/Native.Framework/wiki/04.-编译部署) 中的步骤, 将SDK编译, 重新部署到 "dev" 目录, 然后使用酷Q进行测试窗体的加载

![效果展示](https://jie2gg.github.io/Image/Native.Framework/Native_ShowWindow.png)