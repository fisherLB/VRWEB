using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
namespace JN.Data.Enum
{
    public enum AdvertiseStatus
    {
        /// <summary>
        /// 交易正在进行
        /// </summary>
        [Description("交易正在进行")]
        Underway = 1,

        /// <summary>
        /// 订单已完成
        /// </summary>
        [Description("订单已完成")]
        Completed = 2,

        /// <summary>
        /// 下架
        /// </summary>
        [Description("下架")]
        Cancel = -1,
     
    }
}
