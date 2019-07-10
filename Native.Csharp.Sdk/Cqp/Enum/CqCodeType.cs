using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Enum
{
    /// <summary>
    /// 表示酷Q消息中内含 [CQ:...] 中的类型
    /// </summary>
    [DefaultValue(CqCodeType.Unknown)]
    public enum CqCodeType
    {
        /// <summary>
        /// 位置类型, 同时也是默认值
        /// </summary>
		Unknown,
        /// <summary>
        /// QQ表情
        /// </summary>
        Face,
        /// <summary>
        /// Emoji表情
        /// </summary>
        Emoji,
        /// <summary>
        /// 原创表情
        /// </summary>
        Bface,
        /// <summary>
        /// 小表情
        /// </summary>
        Sface,
        /// <summary>
        /// 图片
        /// </summary>
        Image,
        /// <summary>
        /// 语音
        /// </summary>
        Record,
        /// <summary>
        /// At默认
        /// </summary>
        At,
        /// <summary>
        /// 猜拳魔法表情
        /// </summary>
        Rps,
        /// <summary>
        /// 掷骰子魔法表情
        /// </summary>
        Dice,
        /// <summary>
        /// 戳一戳
        /// </summary>
        Shake,
        /// <summary>
        /// 音乐
        /// </summary>
        Music,
        /// <summary>
        /// 链接分享
        /// </summary>
        Share,
        /// <summary>
        /// 卡片消息
        /// </summary>
        Rich,
        /// <summary>
        /// 签到
        /// </summary>
        Sign,
        /// <summary>
        /// 红包
        /// </summary>
        Hb,
        /// <summary>
        /// 推荐
        /// </summary>
        Contact,
        /// <summary>
        /// 厘米秀
        /// </summary>
        Show,
        /// <summary>
        /// 位置分享
        /// </summary>
        Location
    }
}
