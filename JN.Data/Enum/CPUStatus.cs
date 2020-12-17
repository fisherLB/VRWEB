using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum CPUStatus
    {
        /// <summary>
        /// 暂停
        /// </summary>
        [Description("暂停")]
        Stop = 0,

        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Normal = 1,

        /// <summary>
        /// 完成
        /// </summary>
        [Description("完成")]
        Complete = 2,
    }
}