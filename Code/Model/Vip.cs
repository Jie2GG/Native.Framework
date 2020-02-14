using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Model
{
    public class Vip
    {
        /// <summary>
        /// QQ等级
        /// </summary>
        public int QqLevel { get; set; }

        /// <summary>
        /// 等级加速度
        /// </summary>
        public double LevelSpeed { get; set; }

        /// <summary>
        /// 会员等级
        /// </summary>
        public string VipLevel { get; set; }

        /// <summary>
        /// 会员成长速度
        /// </summary>
        public double GrowSpeed { get; set; }

        /// <summary>
        /// 会员成长总值
        /// </summary>
        public long GrowupTotal { get; set; }
    }
}
