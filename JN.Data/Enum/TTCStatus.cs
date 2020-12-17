using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum TTCStatus
    {
        /// <summary>
        /// 委托中
        /// </summary>
        [Description("委托中")]
        Entrusts = 0,

        /// <summary>
        /// 部分成交
        /// </summary>
        [Description("部分成交")]
        PartOfTheDeal = 1,

        /// <summary>
        /// 全部成交
        /// </summary>
        [Description("全部成交")]
        AllDeal = 2,

        /// <summary>
        /// 已撤销
        /// </summary>
        [Description("已撤销")]
        Cancel = -1,
    }
}