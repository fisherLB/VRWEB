using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //会员等级
    public enum UserLevel
    {
        /// <summary>
        /// 普通会员
        /// </summary>
        [Description("普通会员")]
        Level0 = 0,
        /// <summary>
        /// 矿工
        /// </summary>
        [Description("矿工")]
        Level1 = 1,
        /// <summary>
        /// 高级矿工
        /// </summary>
        [Description("高级矿工")]
        Level2 = 2,
        /// <summary>
        /// 矿场
        /// </summary>
        [Description("矿场")]
        Level3 = 3,
        /// <summary>
        /// 高级矿场
        /// </summary>
        [Description("高级矿场")]
        Level4 = 4,
        /// <summary>
        /// 矿池
        /// </summary>
        [Description("矿池")]
        Level5 = 5,
        /// <summary>
        /// 高级矿池
        /// </summary>
        [Description("高级矿池")]
        Level6 = 6,
        /// <summary>
        /// 资深矿池
        /// </summary>
        [Description("资深矿池")]
        Level7 = 7,
        /// <summary>
        /// 特资深矿池
        /// </summary>
        [Description("特资深矿池")]
        Level8 = 8,
    }
}