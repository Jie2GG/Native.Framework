using Native.Csharp.Sdk.Cqp.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
    /// <summary>
    /// <para>消息体</para>
    /// <para>存储进行解析的消息体结构</para>
    /// </summary>
    public class MessageContent
    {
        /// <summary>
        /// <para>消息体类型</para>
        /// </summary>
        public ContentType Type { get; set; } = ContentType.Text;

        /// <summary>
        /// <para>类型定义号</para>
        /// <list type="table">
        /// <item><description><para><see cref="Enum.ContentType.BFace" langword="个性表情包编号(PackageID)"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Music" langword="音乐服务提供方定义号"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Dict" langword="定义数字"/></para></description></item>
        /// </list>
        /// </summary>
        public string TypeId { get; set; }

        /// <summary>
        /// <para>消息体编号</para>
        /// <list type="table">
        /// <item><description><para><see cref="Enum.ContentType.At" langword="对象的QQ号"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Face" langword="基本表情编号"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.BFace" langword="个性表情编号"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Emoji" langword="Emoji编号(UtfCode)"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Share" langword="分享连结的图片路径"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Music" langword="关连的音乐编号"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Show" langword="秀秀编号"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Contact_Qq" langword="对象的QQ号"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Contact_Group" langword="对象的群号"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Gift" langword="对象的QQ号"/></para></description></item>
        /// </list>
        /// </summary>
        public string TargetId { get; set; }

        /// <summary>
        /// <para>消息体标题</para>
        /// <list type="table">
        /// <item><description><para><see cref="Enum.ContentType.Hb" langword="红包标题"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Share" langword="分享连结标题"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Sign" langword="签到标题"/></para></description></item>
        /// </list>
        /// </summary>
        public string TargetTitle { get; set; }

        /// <summary>
        /// <para>消息体连结</para>
        /// <list type="table">
        /// <item><description><para><see cref="Enum.ContentType.Image" langword="图片档案名称"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Record" langword="语音档案名称"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Record" langword="分享连结路径"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Record" langword="签到图片路径"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Rich" langword="文本连结路径"/></para></description></item>
        /// </list>
        /// </summary>
        public string TargetUrl { get; set; }

        /// <summary>
        /// <para>消息体内文</para>
        /// <list type="table">
        /// <item><description><para><see cref="Enum.ContentType.Hb" langword="红包名称"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Gift" langword="礼物名称"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Emoji" langword="原始Emoji"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Sign" langword="坐标地址"/></para></description></item>
        /// <item><description><para><see cref="Enum.ContentType.Rich" langword="文本内文"/></para></description></item>
        /// </list>
        /// </summary>
        public string Content { get; set; }
    }
}
