using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Csharp.Sdk.Cqp.Model;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
    /// <summary>
    /// 表示群组文件上传事件参数的类
    /// </summary>
    public class CqGroupFileUploadEventArgs : CqEventArgsBase
    {
        /// <summary>
        /// 获取一个值, 该值表示当前事件的类型
        /// </summary>
        public override int Type { get { return 11; } }

        /// <summary>
        /// 获取当前文件上传时间
        /// </summary>
        public DateTime SendTime { get; private set; }

        /// <summary>
        /// 获取当前消息的来源群组号
        /// </summary>
        public long FromGroup { get; private set; }

        /// <summary>
        /// 获取当前消息的来源QQ号
        /// </summary>
        public long FromQQ { get; private set; }

        /// <summary>
        /// 获取当前上传的文件详细信息
        /// </summary>
        public GroupFile File { get; private set; }

        /// <summary>
        /// 获取或设置一个值, 指示当前是否处理过此事件. 若此值为 True 将停止处理后续事件
        /// </summary>
        public bool Handler { get; set; }

        /// <summary>
        /// 初始化 <see cref="CqGroupFileUploadEventArgs"/> 类的一个新实例
        /// </summary>
        /// <param name="id">事件ID</param>
        /// <param name="name">事件名称</param>
        /// <param name="sendTime">发送时间</param>
        /// <param name="fromGroup">来源群</param>
        /// <param name="fromQQ">来源QQ</param>
        /// <param name="file">文件信息</param>
        public CqGroupFileUploadEventArgs (int id, string name, DateTime sendTime, long fromGroup, long fromQQ, GroupFile file)
        {
            base.Id = id;
            base.Name = name;
            this.SendTime = sendTime;
            this.FromGroup = fromGroup;
            this.FromQQ = fromQQ;
            this.File = file;
            this.Handler = false;
        }
    }
}
