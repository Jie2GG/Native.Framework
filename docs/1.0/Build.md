## 简介

通过简单的代码编写, 我们已经完成了对 **复读机** 应用的开发, 现在我们需要将 SDK 编译后部署到酷Q中, 使其运行起来

## 注意事项

* 编译前, 必须保证所有第三方动态库都有在 **Native.Core** 中引用
* 编译 SDK 必须从 **Native.Core** 项目开始生成
* 所有的新建项目必须配置 x86 模式
* 在正式发布 SDK 时, 必须转换为 **Release** 模式, 以去除 SDK 对酷Q数据的校验产生的异常

## 配置管理器

* 打开菜单 "生成", 选择最后一项 "配置管理器"
* 为所有 **AnyCPU** 的项目新建 x86 生成方式
* 确保所有项目的 **生成** 都处于勾选状态

    ![配置管理器](https://jie2gg.github.io/Image/Native.Framework/Native_SelectConfigManager.png)
    ![设置x86](https://jie2gg.github.io/Image/Native.Framework/Native_CreateX86Platform.png)

## 编译项目

* 选择 **Native.Core** 项目, 右键选择 "重新生成", 以如下生成结果为生成成功

    ![选择重新生成](https://jie2gg.github.io/Image/Native.Framework/Native_SelectCompile.png)
    ![编译成功](https://jie2gg.github.io/Image/Native.Framework/Native_Compile.png)

## 部署到酷Q

* 将生成结果 com.jiegg.demo 文件夹 与 com.jiegg.demo.pdb 赋值到 **酷Q** 的 dev 目录下
* 打开酷Q主程序, 登录后日志显示 com.jiegg.demo 已加载, 并且应用管理下也出现了 "复读机应用 for C#" 应用表示成功

将插件启用之后就可以在日志窗口和群内观察到效果

![复制到dev](https://jie2gg.github.io/Image/Native.Framework/Native_CopyToDev.png)
![加载成功](https://jie2gg.github.io/Image/Native.Framework/Native_PluginList.png)