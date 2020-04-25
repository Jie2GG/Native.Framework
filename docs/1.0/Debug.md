## 简介

由于 .Net 类库应用的特殊性, 在使用 **Native.SDK** 开发时, 调试代码需要通过 **前置程序** 才可以完成. 由于 **酷Q** 是标准的Win32应用程序, 在调试的时候需要使用 **酷Q** 配合调试

## 注意事项

* VS必须处于 Debug x86 模式下, 具体操作可以查看 [04. 编译部署](/Jie2GG/Native.Framework/wiki/04.-编译部署)
* dev 目录下必须有对应应用的 AppID.pdb (本例: com.jiegg.demo.pdb)
* 酷Q应用必须处于启用状态
* 请尽量保持酷Q只有正在开发的应用, 没有其它应用

## 调试过程

* 将 **Native.Core** 生成路径下的 "AppID" 文件夹和 "AppID.pdb" 复制到酷Q的 "dev" 文件夹下
* 点击 **酷Q** 的 **重载应用** 按钮, 重载复制到 "dev" 文件夹下的应用

    ![复制文件](https://jie2gg.github.io/Image/Native.Framework/Native_DebugCopy.png)
    ![重载应用](https://jie2gg.github.io/Image/Native.Framework/Native_DebugReloading.png)

* 选择 "调试" 菜单, 之后点击 "附加到进程按钮"
* 在进程搜索框输入 "CQ", 选择类型为 **"托管"** 的 "CQX.exe" 进程, 然后点击 "附加" 按钮

    ![选择菜单](https://jie2gg.github.io/Image/Native.Framework/Native_DebugSelectMenu.png)
    ![附加进程](https://jie2gg.github.io/Image/Native.Framework/Native_DebugAdditionalCQ.png)

* 在 **AppID.UI** 中下一个断点 (使用菜单事件做断点比较好测试)
* 点击对应的菜单, 会发现成功进入断点, 表示成功进入调试模式

    ![下断点](https://jie2gg.github.io/Image/Native.Framework/Native_DebugSwitch.png)
    ![进入断点](https://jie2gg.github.io/Image/Native.Framework/Native_DebugSuccess.png)