using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Model
{
    public class Notice
    {
        /// <summary>
        /// 发布人
        /// </summary>
        public long PublicQqId { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime PublicDateTime { get; set; }

        /// <summary>
        /// 发布信息
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 发布内文
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 发布图片编号
        /// </summary>
        public List<string> PictureId { get; set; }

        /// <summary>
        /// 发布类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 发级新会员
        /// </summary>
        public bool IsForNewMember { get; set; }

        /// <summary>
        /// 发布编号
        /// </summary>
        public string NoticId { get; set; }

        /// <summary>
        /// 阅读人数
        /// </summary>
        public long ReadNum { get; set; }
    }
}
