## 简介

在酷Q应用开发中, Json 的配置关系到后续开发的事件能否被正确调用. 在阅读本章节之前, 建议先了解 [app.json](https://docs.cqp.im/dev/v9/app.json/)、[事件(event)](https://docs.cqp.im/dev/v9/app.json/event/) 之后继续阅读

## 注意事项

* app.json 支持注释功能, 既 "//" 开头作为注释
* app.json 的文件编码为 UTF-8, SDK已经默认转换为此编码, 保存时请注意编码格式
* app.json 中允许出现重复类型的事件, 但是必须以不同的 Id 区分
* app.json 只要一经修改, 就必须要重新运行 "CQExport.tt" 文件

### 修改完毕后, 请重新运行 CQExport.tt 文件, 以保证酷Q可以正常和SDK通讯！

## 配置Json

请根据后续的配置规范理解配置的意义

```JSON
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
    "menu": [],
    "status": [],
    "auth": [
        101	//发送群消息
    ]
}
```

## 配置规范

下述 **event**、**menu**、**status**、**auth** 节点, 分别定义了SDK向酷Q公开的函数, 以及需要的权限, 请按照需要改动.

* event节点的配置

```JSON
"event": [
    {
        "id": 1,                          //事件ID，从1开始，不可重复
        "type": 21,                       //事件类型，后续有讲述这里的值该如何填写
        "name": "私聊消息处理",             //事件名称
        "function": "_eventPrivateMsg",   //事件函数名，务必使用英文
        "priority": 30000                 //事件优先级，数字越小优先级越高
    }
    //如果有多个事件，应按照上述“{}”中的内容填写完整，并用“,”隔开
]
```

* menu节点的配置

```JSON
"menu": [
    {
        "name": "设置A",        //菜单名称
        "function": "_menuA"   //菜单对应函数，务必使用英文
    }
    //如果有多个事件，应按照上述“{}”中的内容填写完整，并用“,”隔开
]
```

* status节点的配置

```JSON
"status": [
    {
        "id": 1,                        //悬浮窗ID
        "name": "运行时间",              //悬浮窗名称，会在菜单展示
        "title": "UPTIME",              //悬浮窗标题，展示在悬浮窗的英文名称
        "function": "_statusUptime",    //事件函数名，务必使用英文
        "period": "1000"                //更新间隔(单位ms(毫秒)，目前仅支持1000ms)
    }
    //如果有多个事件，应按照上述“{}”中的内容填写完整，并用“,”隔开
]
```

* auth节点的配置

```JSON
"auth": [
    20,
    30
    //如果应用需要多种权限，应按照上述方式填写以“,”隔开
]
```

## 事件类型

以下类型均在 app.json 的 event 节点下使用, ID表示每个元素的 "type" 的值

* 酷Q框架事件

    |ID|说明|描述
    |-|-|-
    |1001|酷Q启动事件|该事件将在酷Q启动时调用一次
    |1002|酷Q退出事件|该事件将在酷Q即将退出时调用一次
    |1003|应用启用事件|该事件将在被点击 "启用" 时被调用一次
    |1004|应用停用事件|该事件将在被点击 "停用" 时被调用一次

* 消息接收事件

    |ID|说明|描述
    |-|-|-
    |2|群消息事件|
    |4|讨论组消息事件|
    |11|群文件上传事件|
    |21|私聊消息事件|收到：好友消息、群临时私聊、讨论组临时私聊、临时消息、在线状态 时被调用

* 群管理类事件

    |ID|说明|描述
    |-|-|-
    |101|群管理员变动事件|群管理员增加、群管理员减少 时被调用
    |102|群成员减少事件|群成员被踢、群成员退群 时被调用
    |103|群成员增加事件|群成员主动加群、群成员被邀请入群 时调用
    |104|群禁言事件|群或群成员被 禁言、解除禁言 时调用

* 请求类事件

    |ID|说明|描述
    |-|-|-
    |201|好友已添加事件|当已经成为好友时, 触发该事件
    |301|好友添加请求|收到好友请求时, 触发该事件
    |302|群添加请求|收到群添加请求时, 触发该事件


## 应用权限 (auth字段)

|ID|敏感|说明|函数
|-|:-:|-|-
|20|√|取Cookies|getCookies / getCsrfToken
|30||接收语音|getRecord
|101||发送群消息|sendGroupMsg
|103||发送讨论组消息|sendDiscussMsg
|106||发送私聊消息|sendPrivateMsg
|110|√|发送赞|sendLike
|120||置群员移除|setGroupKick
|121||置群员禁言|setGroupBan
|122||置群管理员|setGroupAdmin
|123||置全群禁言|setGroupWholeBan
|124||置匿名群员禁言|setGroupAnonymousBan
|125||置群匿名设置|setGroupAnonymous
|126||置群成员名片|setGroupCard
|127|√|置群退出|setGroupLeave
|128||置群成员专属头衔|setGroupSpecialTitle
|130||取群成员信息|getGroupMemberInfo
|131||取陌生人信息|getStrangerInfo
|132||取群信息|getGroupInfo
|140||置讨论组退出|setDiscussLeave
|150||置好友添加请求|setFriendAddRequest
|151||置群添加请求|setGroupAddRequest
|160||取群成员列表|getGroupMemberList
|161||取群列表|getGroupList
|162||取好友列表|getFriendList
|180||撤回消息|deleteMsg