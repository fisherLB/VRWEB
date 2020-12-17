using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    public enum RechargeSatus
    {
        /// <summary>
        /// 待确认
        /// </summary>
        [Description("待确认")]
        Wait = 1,
        /// <summary>
        /// 充值完成
        /// </summary>
        [Description("充值完成")]
        Sucess = 2,
        /// <summary>
        /// 充值失败
        /// </summary>
        [Description("充值失败")]
        Fail = -1,
        /// <summary>
        /// 未充值
        /// </summary>
        [Description("未充值")]
        Un = 0
    }
}