using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum MatchingStatus
    {
        /// <summary>
        /// 未付款
        /// </summary>
        [Description("未付款")]
        UnPaid = 1,
        /// <summary>
        /// 延迟付款
        /// </summary>
        [Description("延迟付款")]
        Delayed = 2,
        /// <summary>
        /// 已付款
        /// </summary>
        [Description("已付款")]
        Paid = 3,
        /// <summary>
        /// 已成交
        /// </summary>
        [Description("已成交")]
        Verified = 4,
        /// <summary>
        /// 已评价
        /// </summary>
        [Description("已评价")]
        Evaluate = 5,
        /// <summary>
        /// 取消
        /// </summary>
        [Description("取消")]
        Cancel = -1,
        /// <summary>
        /// 虚假信息
        /// </summary>
        [Description("虚假信息")]
        Falsehood = -2,
        /// <summary>
        /// 超时未付款
        /// </summary>
        [Description("超时未付款")]
        TimeOut = -3,
    }
}