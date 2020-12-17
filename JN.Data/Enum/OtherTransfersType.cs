using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum OtherTransfersType
    {
        [Description("转入")]
        IN = 1,

        [Description("转出")]
        OUT = 2,

    }
}