using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Model
{
    public class Friend
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// QQ号
        /// </summary>
        public long QqId { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 分组顺序
        /// </summary>
        public int GroupIndex { get; set; }

        /// <summary>
        /// 会员
        /// </summary>
        public bool IsVip { get; set; }

        /// <summary>
        /// 会员等级
        /// </summary>
        public int VipLevel { get; set; }

        /// <summary>
        /// 个人备注信息
        /// </summary>
        public string Remark { get; set; }
    }
}
