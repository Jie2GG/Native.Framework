## 简介

**IniCofnig** 是基于 **正则表达式** 应用于  .Net 平台的 Ini配置文件工具类, 在不依赖于 WindowsApi 的同时能快速解析配置文件的 "节点"、"键"、"值" 和 "注释", 转换为对象模型, 轻松实现针对 ini 的增删改查.

## 主要类

* IniCovert - 关于 "IniObject" 和模型类进行互转的类
* IniObject - 描述 **Ini配置文件** 的类
* IniSection - 描述 **Ini配置文件** 中 "节" 的类
* IniValue - 描述 **Ini配置文件** 中值, 提供基础数据类型到 **Ini值** 的转换和描述类

## 在 C#/VB.NET 中实现对 IniConfig 的使用

* 创建对象保存到文件
    * 在C#中使用如下代码:
        ```C#
        // 标准方式
        IniSection section1 = new IniSection ("节点1");    // 创建节点时指定节点名称, 相当于 [节点1]
        section1.Add ("Username", "Jie2GG");              // 添加一个键值对, 相当于 Username=Jie2GG
        section1.Add ("IsManager", true);
        section1.Add ("Level", 20);
        section1.Add ("JoinDate", DateTime.Now);          // 时间类型在这里会自动转换

        IniObject iniFile = new IniObject ();
        iniFile.Add (section1);    // 将节点添加到 object 中
        iniFile.Save ("test.ini"); // 该参数可以是绝对路径也可以是相对路径

        // 语法糖
        iniFile = new IniObject ()
        {
            new IniSection ("节点1")
            {
                { "Username", "Jie2GG" },
                { "IsManager", true },
                { "Level", 20 },
                { "JoinDate", DateTime.Now }
            }
        };
        ```
    * 在VB中使用如下代码
        ```VB
        ' 标准方式
        Dim section1 As IniSection = New IniSection("节点1")
        section1.Add("Username", "Jie2GG")
        section1.Add("IsManager", True)
        section1.Add("Level", 20)
        section1.Add("JoinDate", DateTime.Now)

        Dim iniFile As IniObject = New IniObject()
        iniFile.Add(section1)
        iniFile.Save("test.ini")

        ' 语法糖
        iniFile = New IniObject From {
            New IniSection("节点1") From {
                {"Username", "Jie2GG"},
                {"IsManager", True},
                {"Level", 20},
                {"JoinDate", DateTime.Now}
            }
        }
        iniFile.Save("test.ini")
        ```

* 从文件中读取对象
    * 在C#中使用如下代码:
        ```C#
        IniObject iniFile = IniObject.LoadOrCreate ("test.ini");
        ```
    * 在VB中使用如下代码:
        ```VB
        Dim iniFile = IniObject.LoadOrCreate("test.ini")
        ```

* 查询文件对象
    * 在C#中使用如下代码:
        ```C#
        IniObject iniFile = IniObject.LoadOrCreate ("test.ini");
        // 方式一
        // 判断是否存在 "节点1"
        if (iniFile.ContainsKey ("节点1"))
        {
            IniValue iniValue = iniFile["节点1"]["Username"];            // 获取 "Username" 的值, 返回的是 IniValue 对象
            string username = iniValue.Value;                           // 获取字符串
            bool isManager = iniFile["节点1"]["IsManager"].ToBoolean (); // 获取布尔值
            int level = -1;
            // 为了代码安全, 还可以判断是否存在键
            if (iniFile["节点1"].ContainsKey ("Level"))
            {
                level = iniFile["节点1"]["Level"].ToInt32 ();
            }
        }

        // 方式二
        IniSection section1 = null;
        if (iniFile.TryGetValue ("节点1", out section1))
        {
            IniValue iniValue = null;
            if (section1.TryGetValue ("Username", out iniValue))
            {
                string username = iniValue.Value;
            }
            if (section1.TryGetValue ("IsManager", out iniValue))
            {
                bool isManager = iniValue.ToBoolean ();
            }
        }
        ```
    * 在VB中使用如下代码:
        ```VB
        Dim iniFile = IniObject.LoadOrCreate("test.ini")
        ' 方式一
        ' 判断是否存在 "节点1"
        If iniFile.ContainsKey("节点1") Then
            Dim iniValue As IniValue = iniFile("节点1")("Username")              ' 获取 "Username" 的值, 返回的是 IniValue 对象
            Dim username As String = iniValue.Value                             ' 获取字符串
            Dim isManager As Boolean = iniFile("节点1")("IsManager").ToBoolean() ' 获取布尔值
            Dim level As Integer = -1
            ' 为了代码安全, 还可以判断是否存在键
            If iniFile("节点1").ContainsKey("Level") Then
                level = iniFile("节点1")("Level").ToInt32()
            End If
        End If

        ' 方式二
        Dim section1 As IniSection = Nothing
        If iniFile.TryGetValue("节点1", section1) Then
            Dim iniValue As IniValue = Nothing
            If section1.TryGetValue("Username", iniValue) Then
                Dim username As String = iniValue.Value
            End If
            If section1.TryGetValue("IsManager", iniValue) Then
                Dim isManager As Boolean = iniValue.ToBoolean()
            End If
        End If
        ```

* 修改文件对象
    * 在C#中使用如下代码:
        ```C#
        ```
    * 在VB中使用如下代码:
        ```VB
        ```