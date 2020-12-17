using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum OtherTransfersStatus
    {
        [Description("待确认")]
        Wait = 0,

        [Description("成功")]
        Sucess = 10,

    }
}