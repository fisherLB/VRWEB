using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    public enum TakeCaseStatus
    {
        /// <summary>
        /// 待审核
        /// </summary>
        [Description("待审核")]
        Wait = 0,
        /// <summary>
        /// 审核锁定
        /// </summary>
        [Description("审核锁定")]
        Passed = 1,
        /// <summary>
        /// 已支付
        /// </summary>
        [Description("已支付")]
        Payed = 2,
        /// <summary>
        /// 已完成
        /// </summary>
        [Description("已完成")]
        Deal = 3,
        /// <summary>
        /// 已取消
        /// </summary>
        [Description("已取消")]
        Cancel = -1,

        /// <summary>
        /// 拒绝
        /// </summary>
        [Description("拒绝")]
        Refusal = -1
    }
}