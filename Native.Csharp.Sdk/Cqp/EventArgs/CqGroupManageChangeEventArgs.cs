using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
    /// <summary>
    /// 表示群管理变动事件参数的类
    /// </summary>
    public class CqGroupManageChangeEventArgs : CqEventArgsBase
    {
        /// <summary>
        /// 获取一个值, 该值表示当前事件的类型
        /// </summary>
        public override int Type { get { return 101; } }

        /// <summary>
        /// 获取当前事件触发时间
        /// </summary>
        public DateTime SendTime { get; private set; }

        /// <summary>
        /// 获取当前消息的来源群组号
        /// </summary>
        public long FromGroup { get; private set; }

        /// <summary>
        /// 获取当前事件触发时的目标QQ
        /// </summary>
        public long BeingOperateQQ { get; private set; }

        /// <summary>
        /// 获取或设置一个值, 指示当前是否处理过此事件. 若此值为 True 将停止处理后续事件
        /// </summary>
        public bool Handler { get; set; }

        /// <summary>
        /// 初始化 <see cref="CqGroupManageChangeEventArgs"/> 类的一个新实例
        /// </summary>
        /// <param name="id">事件ID</param>
        /// <param name="name">事件名称</param>
        /// <param name="sendTime">触发时间</param>
        /// <param name="fromGroup">来源群</param>
        /// <param name="targetQQ">目标QQ</param>
        public CqGroupManageChangeEventArgs (int id, string name, DateTime sendTime, long fromGroup, long targetQQ)
        {
            base.Id = id;
            base.Name = name;
            this.SendTime = sendTime;
            this.FromGroup = fromGroup;
            this.BeingOperateQQ = targetQQ;
        }
    }
}
