## 简介

本章总结了各位开发者在开发中容易出现的一些问题及解决方案. 为开发者提供快速问题查询通道.

同时也欢迎开发者通过[Issues](https://github.com/Jie2GG/Native.Framework/issues)或者[QQ群](http://qm.qq.com/cgi-bin/qm/qr?k=HT_Ei-HIuCnVRIyMzB-CQQXeF80ZyUC-&group_code=711841640)进行反馈

## 常见错误

> 为什么酷Q日志上显示已发送消息, 但是QQ上未显示消息

答: 酷Q属于第三方客户端, 发送的消息可能被TX风控, 请在服务器上使用官方客户端挂一段时间并绑定设备锁. 然后再使用酷Q

> 为什么生成结果只有 app.dll 没有 app.json

答: 请查看 **Native.Core** 项目中的 **app.json** 的属性: "始终复制" 是否已为 True

> 为什么突然在使用 **获取xxxx列表**、**获取xxxx信息** 的过程中突然报错?

答: 因为SDK是从酷Q获取的数据, 而这些数据并不是 100% 正确的, 所以SDK在 "Debug" 模式下对酷Q返回的数据进行了校验, 以方便开发者确定错误的源头. 开发者应在开发中对返回的结果进行 null 判断, 在切换回 "Release" 模式后, SDK将消除这些异常的代码. 除错后将返回 null 值.

> 为什么我无法编译下载的 **Native.SDK**, 一编译就出现报错?

答: 请检查是否包含如下异常
```csharp
“Fody.WeavingTask”任务意外失败。
System.TypeInitializationException: “ContainsTypeChecker”的类型初始值设定项引发异常。 --->
 System.IO.FileLoadException: 未能加载文件或程序集“file:///D:\project\Native.Csharp.Frame\packages\Fody.2.2.1.0\FodyIsolated.dll”或它的某一个依赖项。不支持操作。 (异常来自 HRESULT:0x80131515) ---> System.NotSupportedException: 尝试从一个网络位置加载程序集，在早期版本的 .NET Framework 中，这会导致对该程序集进行沙盒处理。此发行版的 .NET Framework 默认情况下不启用 CAS 策略，因此，此加载可能会很危险。如果此加载不是要对程序集进行沙盒处理，请启用 loadFromRemoteSources 开关。有关详细信息，请参见 http://go.microsoft.com/fwlink/?LinkId=155569。

   --- 内部异常堆栈跟踪的结尾 ---
   在 System.Reflection.RuntimeAssembly._nLoad(AssemblyName fileName, String codeBase, Evidence assemblySecurity, RuntimeAssembly locationHint, StackCrawlMark& stackMark, IntPtr pPrivHostBinder, Boolean throwOnFileNotFound, Boolean forIntrospection, Boolean suppressSecurityChecks)
   在 System.Reflection.RuntimeAssembly.nLoad(AssemblyName fileName, String codeBase, Evidence assemblySecurity, RuntimeAssembly locationHint, StackCrawlMark& stackMark, IntPtr pPrivHostBinder, Boolean throwOnFileNotFound, Boolean forIntrospection, Boolean suppressSecurityChecks)
   在 System.Reflection.RuntimeAssembly.InternalLoadAssemblyName(AssemblyName assemblyRef, Evidence assemblySecurity, RuntimeAssembly reqAssembly, StackCrawlMark& stackMark, IntPtr pPrivHostBinder, Boolean throwOnFileNotFound, Boolean forIntrospection, Boolean suppressSecurityChecks)
   在 System.Reflection.RuntimeAssembly.InternalLoadFrom(String assemblyFile, Evidence securityEvidence, Byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm, Boolean forIntrospection, Boolean suppressSecurityChecks, StackCrawlMark& stackMark)
   在 System.Reflection.Assembly.LoadFrom(String assemblyFile, Evidence securityEvidence)
   在 System.Activator.CreateInstanceFromInternal(String assemblyFile, String typeName, Boolean ignoreCase, BindingFlags bindingAttr, Binder binder, Object[] args, CultureInfo culture, Object[] activationAttributes, Evidence securityInfo)
   在 System.AppDomain.CreateInstanceFrom(String assemblyFile, String typeName)
   在 System.AppDomain.CreateInstanceFromAndUnwrap(String assemblyName, String typeName)
   在 System.AppDomain.CreateInstanceFromAndUnwrap(String assemblyName, String typeName)
   在 ContainsTypeChecker..cctor()
   --- 内部异常堆栈跟踪的结尾 ---
   在 ContainsTypeChecker..ctor()
   在 Processor..ctor()
   在 Fody.WeavingTask.Execute()
   在 Microsoft.Build.BackEnd.TaskExecutionHost.Microsoft.Build.BackEnd.ITaskExecutionHost.Execute()
   在 Microsoft.Build.BackEnd.TaskBuilder.<ExecuteInstantiatedTask>d__26.MoveNext()	Native.Csharp			

```
上述错误缘由触发了 CAS 策略, 需要接触被锁定的文件. 例如本异常需要解锁的文件为 `file:///D:\project\Native.Csharp.Frame\packages\Fody.2.2.1.0\FodyIsolated.dll`

* 其它问题正在收集中...