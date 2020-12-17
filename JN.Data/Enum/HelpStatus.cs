using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum HelpStatus
    {
        /// <summary>
        /// 未匹配
        /// </summary>
        [Description("未匹配")]
        NoMatching = 1,

        /// <summary>
        /// 部分匹配
        /// </summary>
        [Description("部分匹配")]
        PartOfMatching = 2,

        /// <summary>
        /// 全部匹配
        /// </summary>
        [Description("全部匹配")]
        AllMatching = 3,


        /// <summary>
        /// 部分成交
        /// </summary>
        [Description("部分成交")]
        PartOfDeal = 4,


        /// <summary>
        /// 全部成交
        /// </summary>
        [Description("全部成交")]
        AllDeal = 5,

        /// <summary>
        /// 已领取收益
        /// </summary>
        [Description("已领取收益")]
        receiveIncome = 6,


        /// <summary>
        /// 已撤消
        /// </summary>
        [Description("已撤消")]
        Cancel = -1,
    }
}