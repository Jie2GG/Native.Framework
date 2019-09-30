using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
    /// <summary>
    /// 表示添加群请求事件参数的类
    /// </summary>
    public class CqAddGroupRequestEventArgs : CqEventArgsBase
    {
        /// <summary>
        /// 获取一个值, 该值表示当前事件的类型
        /// </summary>
        public override int Type { get { return 302; } }

        /// <summary>
        /// 获取当前事件触发时间
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
        /// 获取当前消息的消息内容
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// 反馈标识, 用于对该请求做响应时的标识参数
        /// </summary>
        public string ResponseFlag { get; private set; }

        /// <summary>
        /// 获取或设置一个值, 指示当前是否处理过此事件. 若此值为 True 将停止处理后续事件
        /// </summary>
        public bool Handler { get; set; }

        /// <summary>
        /// 初始化 <see cref="CqAddGroupRequestEventArgs"/> 类的一个新实例
        /// </summary>
        /// <param name="id">事件ID</param>
        /// <param name="name">事件名称</param>
        /// <param name="sendTime">触发时间</param>
        /// <param name="fromGroup">来源群组</param>
        /// <param name="fromQQ">来源QQ</param>
        /// <param name="message">附加消息</param>
        /// <param name="flag">反馈标识</param>
        public CqAddGroupRequestEventArgs (int id, string name, DateTime sendTime, long fromGroup, long fromQQ, string message, string flag)
        {
            base.Id = id;
            base.Name = name;
            this.SendTime = sendTime;
            this.FromGroup = fromGroup;
            this.FromQQ = fromQQ;
            this.Message = message;
            this.ResponseFlag = flag;
        }
    }
}
