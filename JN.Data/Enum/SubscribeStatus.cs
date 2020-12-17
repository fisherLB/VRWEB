using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum SubscribeStatus
    {
        [Description("待解冻")]
        NoUsed = 0,

        [Description("部分解冻")]
        PartOfUsed = 1,

        [Description("全部解冻")]
        AllUsed = 2,
    }
}