using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
namespace JN.Data.Enum
{
    public enum AdvertiseOrderStatus
    {
        /// <summary>
        /// 已下单待付款
        /// </summary>
        [Description("已下单待付款")]
        PlaceOrder = 1,

        /// <summary>
        /// 已付款待收货
        /// </summary>
        [Description("已付款待收货")]
        PendingPayment =2,

        /// <summary>
        /// 已收货待评价
        /// </summary>
        [Description("已收货待评价")]
        GoodsReceived= 3,

       
        /// <summary>
        /// 已评价
        /// </summary>
        [Description("已评价")]
        PendingEvalu = 4,


        /// <summary>
        /// 已完成
        /// </summary>
        [Description("已完成")]
        Completed = 5,

        /// <summary>
        /// 已取消
        /// </summary>
        [Description("已取消")]
        Cancel = -1,

        /// <summary>
        /// 超时未确认
        /// </summary>
        [Description("超时未确认")]
        TimeoutUnConfirmed = -2,

    }
}
