using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.Enum
{
    /// <summary>
    /// 音乐类型
    /// </summary>
    [DefaultValue (MusicType.Tencent)]
    public enum MusicType
    {
        /// <summary>
        /// QQ 音乐
        /// </summary>
        [Description ("qq")]
        Tencent,
        /// <summary>
        /// 网易云音乐
        /// </summary>
        [Description ("163")]
        Netease,
        /// <summary>
        /// 虾米音乐
        /// </summary>
        [Description ("xiami")]
        XiaMi
    }
}
