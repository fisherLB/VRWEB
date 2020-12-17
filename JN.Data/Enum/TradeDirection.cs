using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum TradeDirection
    {
        /// <summary>
        /// 买入
        /// </summary>
        [Description("买入")]
        In = 0,

        /// <summary>
        /// 卖出
        /// </summary>
        [Description("卖出")]
        Out = 1,

    }
}