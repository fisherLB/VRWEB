using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //订单状态
    public enum ShopOrderStatus
    {
        [Description("未付款")]
        WithoutPayment = 0,

        [Description("待发货")]
        Sales = 1,

        [Description("已发货")]
        Transaction = 3,

        [Description("已收货")]
        Deal = 4,

        [Description("已取消")]
        Cancel = -1,

        [Description("投诉")]
        Complaint = -2,
    }
}