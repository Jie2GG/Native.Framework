using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
    /// <summary>
    /// 表示群禁言事件参数的类
    /// </summary>
    public class CqGroupBanEventArgs : CqEventArgsBase
    {
        /// <summary>
        /// 表示当前事件的类型
        /// </summary>
        public override int Type { get { return 104; } }

        /// <summary>
        /// 获取当前事件触发时间
        /// </summary>
        public DateTime SendTime { get; private set; }

        /// <summary>
        /// 获取当前消息的来源群组号
        /// </summary>
        public long FromGroup { get; private set; }

        /// <summary>
        /// 操作者QQ
        /// </summary>
        public long FromQQ { get; private set; }

        /// <summary>
        /// 获取当前事件触发时的目标QQ
        /// </summary>
        public long BeingOperateQQ { get; private set; }

        /// <summary>
        /// 禁言时长
        /// </summary>
        public TimeSpan Duration { get; private set; }

        /// <summary>
        /// 获取或设置一个值, 指示当前是否处理过此事件. 若此值为 True 将停止处理后续事件
        /// </summary>
        public bool Handler { get; set; }

        /// <summary>
        /// 初始化 <see cref="CqGroupBanEventArgs"/> 类的新实例
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="sendTime"></param>
        /// <param name="fromGroup"></param>
        /// <param name="fromQQ"></param>
        /// <param name="beingOperateQQ"></param>
        /// <param name="duration"></param>
        public CqGroupBanEventArgs (int id, string name, DateTime sendTime, long fromGroup, long fromQQ, long beingOperateQQ, TimeSpan duration)
        {
            this.Id = id;
            this.Name = name;
            this.SendTime = sendTime;
            this.FromGroup = fromGroup;
            this.FromQQ = fromQQ;
            this.BeingOperateQQ = beingOperateQQ;
            this.Duration = duration;
        }
    }
}
