using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //订单状态
    public enum ShopInfoStatus
    {
        [Description("申请中")]
        Application = 0,

        [Description("营业中")]
        Business = 1,

        [Description("审核不通过")]
        Refuse = -1,

        [Description("冻结中")]
        Frozen = -2,

    }
}