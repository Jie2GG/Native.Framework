using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Enum
{
    public enum ContentType
    {
        [Description("文字")]
        Text,
        [Description("提及我")]
        At,
        [Description("通知全体")]
        All,
        [Description("连结")]
        Share,
        [Description("图片")]
        Image,
        [Description("音乐")]
        Music,
        [Description("语音")]
        Record,
        [Description("魔法语音")]
        Record_Magic,
        [Description("语音红包")]
        Record_Hb,
        [Description("表情")]
        Face,
        [Description("个性表情")]
        BFace,
        [Description("Emoji")]
        Emoji,
        [Description("文本")]
        Rich,
        [Description("推荐联络人")]
        Contact_Qq,
        [Description("推荐群组")]
        Contact_Group,
        [Description("骰子")]
        Dict,
        [Description("签到")]
        Sign,
        [Description("秀秀")]
        Show,
        [Description("视频")]
        Video,
        [Description("震动")]
        Shake,
        [Description("礼物")]
        Gift,
        [Description("闪照")]
        ShiningImage,
        [Description("群公告")]
        GroupNotify,
        [Description("红包")]
        Hb
    }
}
