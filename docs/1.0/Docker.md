## 目标

实现在Linux及纯终端环境下运行酷Q应用的方案在早早前2017年酷Q官方已经实现[具体见此](https://cqp.cc/t/34558)，其透过Wine技术执行酷Q并透过novnc网页端查看界面，又得益於Docker服务我们可以很轻易地部署好实际使用场境。

不过，酷Q官方提供的镜像并不支持.NetFramework，故而我们一众的目标是基於官方镜像增加对运行.Net应用的支持。
  
## 过程

在最初期，我们更改了官方镜像,透过调用net45安装界面实现手动安装，这一流程很是麻烦。故而，有作者提供了预建安装！在使用上如官方镜像一致，无需进行额外步骤，非常感谢[MerCuJerry](https://github.com/MerCuJerry)
  
## 镜像

[mercujerry/wine-coolq-dotnet](https://hub.docker.com/r/mercujerry/wine-coolq-dotnet)是由[MerCuJerry](https://github.com/MerCuJerry)基於官方镜像提供针对``.Net4.5``的应用运行支持所制作的镜像。

## 使用

```docker run -d --name=coolq_dotnet45 -p 8081:9000 -v /root/coolq-data-dotnet45:/home/user/coolq -e COOLQ_URL=http://dlcq.cqp.me/cq/CQP-tuling.zip -e VNC_PASSWD=native mercujerry/wine-coolq-dotnet```

- <kbd>-d</kbd>是指背景运行容器，防止由於脱离终端而结束。
- <kbd>-name</kbd>=coolq_dotnet45 是容器名称,你可自由命名为可识别名称。
- <kbd>-p</kbd>是端口映射，容器内部的``9000``为外部``8081``可访问,而``9000``是novnc网页端。
- <kbd>-v</kbd>是文件映射，容器内部的``/home/user/coolq``存载酷Q的文件夹为外部``/root/coolq-data-dotnet45``之下。
- <kbd>-e</kbd>环境设置，``COOLQ_URL``可指定<kbd>CoolQ Air</kbd>，<kbd>CoolQ Pro</kbd>的下载地址。

於此，我们只需在任一能连通终端8081端口的设备上访问其网页端即可进行操作管理。

![环境](https://i.loli.net/2019/02/20/5c6d1b0627925.png)

