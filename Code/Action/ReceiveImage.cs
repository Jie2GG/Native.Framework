using Native.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Action
{
    public static class ReceiveImage
    {
        public static void ReceiveAllImageFromMessage(QQMessage message)
        {
            //枚举所有图片并遂一下载消息中的图片并保存到本地
            Enumerable.Range(0, message.CQCodes.Where(a => a.Function == Native.Sdk.Cqp.Enum.CQFunction.Image).Count()).ToList().ForEach(i =>
            {
                //使用API将“cqimg”文件转换成图片文件，并返回图片文件路径
                Common.Log.Debug("保存图片", message.ReceiveImage(i));
            });
        }
    }
}
