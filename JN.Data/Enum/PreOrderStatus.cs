using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum PreOrderStatus
    {
        [Description("待买入")]
        NoBuy = 0,

        [Description("部分买入")]
        PartOfBuy = 1,

        [Description("全部买入")]
        AllBuy = 2,

        [Description("已取消")]
        Cancel = -1,
    }
}